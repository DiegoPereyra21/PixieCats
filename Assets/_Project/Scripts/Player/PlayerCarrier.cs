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
    [SerializeField] private float _growSpeed = 6f;

    [Header("Tirar")]
    [SerializeField] private float _dropDistance = 1.2f; // delante del player, para no aparecer dentro de su collider
    [SerializeField] private float _throwForce = 4f;
    [SerializeField] private float _throwUpForce = 1.5f;

    private readonly Stack<PickupObject> _stored = new Stack<PickupObject>();
    private Rigidbody _rb;
    private float _targetScale;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _targetScale = _baseScale;
        _bag.localScale = Vector3.one * _baseScale;
    }

    private void Update()
    {
        // crecimiento suave, independiente del framerate
        float t = 1f - Mathf.Exp(-_growSpeed * Time.deltaTime);
        _bag.localScale = Vector3.Lerp(_bag.localScale, Vector3.one * _targetScale, t);
    }

    public void Store(PickupObject pickup)
    {
        _stored.Push(pickup);
        pickup.OnStored();
        Refresh();
    }

    public void DropLast()
    {
        if (_stored.Count == 0) return;

        PickupObject pickup = _stored.Pop();

        Vector3 position = transform.position + Vector3.up + transform.forward * _dropDistance;
        Vector3 velocity = _rb.linearVelocity + transform.forward * _throwForce + Vector3.up * _throwUpForce;

        pickup.OnDropped(position, velocity);
        Refresh();
    }

    private void Refresh()
    {
        float totalWeight = 0f;
        foreach (PickupObject pickup in _stored) totalWeight += pickup.Weight;

        _targetScale = Mathf.Min(_baseScale + totalWeight * _scalePerWeight, _maxScale);
        _flight.SetCarriedWeight(totalWeight);
    }
}