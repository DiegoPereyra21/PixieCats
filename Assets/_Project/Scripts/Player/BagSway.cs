using UnityEngine;

public class BagSway : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private PlayerFlight _flight;
    [SerializeField] private Transform _bag;

    [Header("Balanceo")]
    [SerializeField] private float _swayPerSpeed = 0.04f; // desplazamiento (m) por cada m/s de velocidad
    [SerializeField] private float _maxSway = 0.8f;
    [SerializeField] private float _stiffness = 40f;      // más alto = vuelve más rápido
    [SerializeField] private float _damping = 6f;         // más bajo = más rebote

    private Vector3 _rest;
    private Vector3 _offset;
    private Vector3 _offsetVelocity;

    private void Awake() => _rest = _bag.localPosition;

    private void LateUpdate()
    {
        float weightFactor = _flight.WeightFactor;

        // la bolsa se queda atrás respecto del movimiento; con peso se mueve menos y más lento
        Vector3 localVelocity = transform.InverseTransformDirection(_rb.linearVelocity);
        Vector3 target = Vector3.ClampMagnitude(-localVelocity * _swayPerSpeed, _maxSway) / weightFactor;

        // resorte subamortiguado; k y c se escalan juntos para mantener el mismo rebote con peso
        float k = _stiffness / weightFactor;
        float c = _damping / Mathf.Sqrt(weightFactor);
        float dt = Mathf.Min(Time.deltaTime, 0.033f); // evita inestabilidad con picos de frame

        Vector3 acceleration = (target - _offset) * k - _offsetVelocity * c;
        _offsetVelocity += acceleration * dt;
        _offset += _offsetVelocity * dt;

        _bag.localPosition = _rest + _offset;
    }
}