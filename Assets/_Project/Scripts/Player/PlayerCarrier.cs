using System.Collections.Generic;
using UnityEngine;

public class PlayerCarrier : MonoBehaviour
{
    [SerializeField] private PlayerFlight _flight;
    [SerializeField] private Transform _bag; // la esfera

    [Header("Tamaño de la bolsa")]
    [SerializeField] private float _baseScale = 0.4f;
    [SerializeField] private float _scalePerWeight = 0.05f;
    [SerializeField] private float _maxScale = 2f;

    [Header("Pulso (resorte)")]
    [SerializeField] private float _springStiffness = 120f;
    [SerializeField] private float _springDamping = 9f; // más bajo = más rebote
    [SerializeField] private float _pulseKick = 3f;     // impulso extra al recibir un objeto

    [Header("Tirar")]
    [SerializeField] private float _dropDistance = 2.2f; // delante del player, para no aparecer dentro de su collider
    [SerializeField] private float _throwForce = 4f;
    [SerializeField] private float _throwUpForce = 1.5f;

    [Header("Eventos (opcionales)")]
    [SerializeField] private VoidEventChannelSO _onItemStored;
    [SerializeField] private VoidEventChannelSO _onItemDropped;

    private readonly Stack<PickupObject> _stored = new Stack<PickupObject>();
    private Rigidbody _rb;
    private float _targetScale;
    private float _scale;
    private float _scaleVelocity;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _targetScale = _baseScale;
        _scale = _baseScale;
        _bag.localScale = Vector3.one * _scale;
    }

    private void Update()
    {
        // resorte subamortiguado: crece pasándose un poco y se asienta
        float dt = Mathf.Min(Time.deltaTime, 0.033f); // evita inestabilidad con picos de frame
        float acceleration = (_targetScale - _scale) * _springStiffness - _scaleVelocity * _springDamping;
        _scaleVelocity += acceleration * dt;
        _scale += _scaleVelocity * dt;
        _bag.localScale = Vector3.one * Mathf.Max(0.01f, _scale);
    }

    public void Store(PickupObject pickup)
    {
        // el objeto vuela a la bolsa y recién al llegar cuenta como guardado
        pickup.AbsorbInto(_bag, () => OnItemArrived(pickup));
    }

    private void OnItemArrived(PickupObject pickup)
    {
        _stored.Push(pickup);
        Refresh();
        _scaleVelocity += _pulseKick;

        if (_onItemStored != null) _onItemStored.Raise();
    }

    public void DropLast()
    {
        if (_stored.Count == 0) return;

        PickupObject pickup = _stored.Pop();

        Vector3 position = transform.position + Vector3.up + transform.forward * _dropDistance;
        Vector3 velocity = _rb.linearVelocity + transform.forward * _throwForce + Vector3.up * _throwUpForce;

        pickup.OnDropped(position, velocity);
        Refresh();

        if (_onItemDropped != null) _onItemDropped.Raise();
    }

    private void Refresh()
    {
        float totalWeight = 0f;
        foreach (PickupObject pickup in _stored) totalWeight += pickup.Weight;

        _targetScale = Mathf.Min(_baseScale + totalWeight * _scalePerWeight, _maxScale);
        _flight.SetCarriedWeight(totalWeight);
    }
}