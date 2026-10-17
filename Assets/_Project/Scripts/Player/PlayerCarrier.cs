using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCarrier : MonoBehaviour
{
    [SerializeField] private PlayerFlight _flight;
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private Transform _bag;                  // raíz del modelo de la bolsa
    [SerializeField] private SkinnedMeshRenderer _bagRenderer; // si está vacío, se busca dentro de _bag
    [SerializeField] private Transform _absorbPoint;          // opcional: boca de la bolsa; si está vacío usa _bag

    [Header("Tamaño de la bolsa (blend shape)")]
    [SerializeField] private string _blendShapeName = "bolsagrande";
    [SerializeField] private float _fullBagWeight = 32f; // peso total con el que la bolsa llega al 100 %
    [SerializeField] private float _minBlend = -20f;     // límites de lo que pueden estirar el pulso y el squash
    [SerializeField] private float _maxBlend = 120f;

    [Header("Pulso (resorte)")]
    [SerializeField] private float _springStiffness = 120f;
    [SerializeField] private float _springDamping = 9f;      // más bajo = más rebote
    [SerializeField] private float _pulseKick = 400f;        // impulso al recibir un objeto (puntos de blend por segundo)
    [SerializeField] private float _takeoffKick = 200f;      // impulso al despegar
    [SerializeField] private float _landSquashPerSpeed = 25f;
    [SerializeField] private float _maxLandSquash = 400f;

    [Header("Peso en vuelo")]
    [SerializeField] private float _sinkPerWeight = 0.8f; // caída (m/s) por punto de peso al recibir un objeto
    [SerializeField] private float _maxSink = 10f;
    [SerializeField] private float _liftPerWeight = 0.4f; // empujón hacia arriba (m/s) por punto de peso al soltar
    [SerializeField] private float _maxLift = 5f;

    [Header("Tirar")]
    [SerializeField] private float _dropDistance = 2.2f; // delante del player, para no aparecer dentro de su collider
    [SerializeField] private float _throwForce = 4f;
    [SerializeField] private float _throwUpForce = 1.5f;

    [Header("Eventos (opcionales)")]
    [SerializeField] private VoidEventChannelSO _onItemStored;
    [SerializeField] private VoidEventChannelSO _onItemDropped;

    // peso del objeto guardado o soltado
    public event Action<float> ItemStored;
    public event Action<float> ItemDropped;

    // el último de la lista es el más reciente
    private readonly List<PickupObject> _stored = new List<PickupObject>();
    private Rigidbody _rb;
    private int _blendIndex = -1;
    private float _targetBlend;
    private float _blend;
    private float _blendVelocity;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        if (_bagRenderer == null) _bagRenderer = _bag.GetComponentInChildren<SkinnedMeshRenderer>();
        FindBlendShape();
        ApplyBlend();
    }

    private void OnEnable()
    {
        if (_movement != null) _movement.Landed += OnLanded;
        if (_flight != null) _flight.TookOff += OnTookOff;
    }

    private void OnDisable()
    {
        if (_movement != null) _movement.Landed -= OnLanded;
        if (_flight != null) _flight.TookOff -= OnTookOff;
    }

    private void Update()
    {
        // resorte subamortiguado: crece pasándose un poco y se asienta
        float dt = Mathf.Min(Time.deltaTime, 0.033f); // evita inestabilidad con picos de frame
        float acceleration = (_targetBlend - _blend) * _springStiffness - _blendVelocity * _springDamping;
        _blendVelocity += acceleration * dt;
        _blend += _blendVelocity * dt;

        ApplyBlend();
    }

    public void Store(PickupObject pickup)
    {
        // el objeto vuela a la bolsa y recién al llegar cuenta como guardado
        Transform target = _absorbPoint != null ? _absorbPoint : _bag;
        pickup.AbsorbInto(target, () => OnItemArrived(pickup));
    }

    public void DropLast()
    {
        if (_stored.Count == 0) return;

        PickupObject pickup = _stored[_stored.Count - 1];
        _stored.RemoveAt(_stored.Count - 1);

        Vector3 position = transform.position + Vector3.up + transform.forward * _dropDistance;
        Vector3 velocity = _rb.linearVelocity + transform.forward * _throwForce + Vector3.up * _throwUpForce;

        pickup.OnDropped(position, velocity);
        Refresh();

        // en vuelo, soltar peso da un empujón hacia arriba (espejo del hundimiento)
        if (_flight.IsFlying)
        {
            float lift = Mathf.Min(pickup.Weight * _liftPerWeight, _maxLift);
            Vector3 current = _rb.linearVelocity;
            _rb.linearVelocity = new Vector3(current.x, current.y + lift, current.z);
        }

        ItemDropped?.Invoke(pickup.Weight);
        if (_onItemDropped != null) _onItemDropped.Raise();
    }

    // Cuenta los objetos guardados que cumplen la condición
    public int CountStored(Predicate<PickupObject> match)
    {
        int count = 0;
        foreach (PickupObject pickup in _stored)
        {
            if (match(pickup)) count++;
        }

        return count;
    }

    // Saca de la bolsa hasta `count` objetos que cumplan la condición (los más recientes primero)
    public List<PickupObject> RemoveStored(Predicate<PickupObject> match, int count)
    {
        List<PickupObject> removed = new List<PickupObject>();

        for (int i = _stored.Count - 1; i >= 0 && removed.Count < count; i--)
        {
            if (!match(_stored[i])) continue;

            removed.Add(_stored[i]);
            _stored.RemoveAt(i);
        }

        Refresh();
        return removed;
    }

    private void OnItemArrived(PickupObject pickup)
    {
        _stored.Add(pickup);
        Refresh();
        _blendVelocity += _pulseKick;

        // en vuelo, el peso hunde al hada de golpe y PlayerFlight la recupera
        if (_flight.IsFlying)
        {
            float sink = Mathf.Min(pickup.Weight * _sinkPerWeight, _maxSink);
            Vector3 velocity = _rb.linearVelocity;
            _rb.linearVelocity = new Vector3(velocity.x, velocity.y - sink, velocity.z);
        }

        ItemStored?.Invoke(pickup.Weight);
        if (_onItemStored != null) _onItemStored.Raise();
    }

    private void OnLanded(float impact) => _blendVelocity -= Mathf.Min(impact * _landSquashPerSpeed, _maxLandSquash);

    private void OnTookOff() => _blendVelocity += _takeoffKick;

    private void Refresh()
    {
        float totalWeight = 0f;
        foreach (PickupObject pickup in _stored) totalWeight += pickup.Weight;

        _targetBlend = Mathf.Clamp01(totalWeight / _fullBagWeight) * 100f;
        _flight.SetCarriedWeight(totalWeight);
    }

    private void FindBlendShape()
    {
        if (_bagRenderer == null || _bagRenderer.sharedMesh == null)
        {
            Debug.LogError("PlayerCarrier: falta el SkinnedMeshRenderer de la bolsa.", this);
            return;
        }

        Mesh mesh = _bagRenderer.sharedMesh;
        _blendIndex = mesh.GetBlendShapeIndex(_blendShapeName);
        if (_blendIndex >= 0) return;

        // si el nombre no coincide, se listan los que tiene el modelo
        string names = "";
        for (int i = 0; i < mesh.blendShapeCount; i++) names += $"\n - {mesh.GetBlendShapeName(i)}";
        Debug.LogError($"PlayerCarrier: no existe el blend shape '{_blendShapeName}'. Los del modelo son:{names}", this);
    }

    private void ApplyBlend()
    {
        if (_bagRenderer == null || _blendIndex < 0) return;

        _bagRenderer.SetBlendShapeWeight(_blendIndex, Mathf.Clamp(_blend, _minBlend, _maxBlend));
    }
}