using Unity.Cinemachine;
using UnityEngine;

public class CameraFlightController : MonoBehaviour
{
    [SerializeField] private CinemachineOrbitalFollow _orbit;
    [SerializeField] private PlayerFlight _flight;

    [Header("En vuelo")]
    [SerializeField] private float _radiusExtra = 2f;   // cuánto se aleja
    [SerializeField] private float _heightExtra = 1.2f; // cuánto sube
    [SerializeField] private float _flyDamping = 2.5f;  // damping de posición (más alto = más retraso)
    [SerializeField] private float _smoothTime = 0.5f;  // tiempo de transición

    private float _baseRadius;
    private Vector3 _baseOffset;
    private Vector3 _baseDamping;
    private float _blend;
    private float _blendVelocity;

    private void Start()
    {
        _baseRadius = _orbit.Radius;
        _baseOffset = _orbit.TargetOffset;
        _baseDamping = _orbit.TrackerSettings.PositionDamping;
    }

    private void LateUpdate()
    {
        // 0 = suelo, 1 = vuelo, con transición suave en ambos sentidos
        _blend = Mathf.SmoothDamp(_blend, _flight.IsFlying ? 1f : 0f, ref _blendVelocity, _smoothTime);

        _orbit.Radius = _baseRadius + _radiusExtra * _blend;
        _orbit.TargetOffset = _baseOffset + Vector3.up * (_heightExtra * _blend);

        // TrackerSettings es un struct: se copia, se modifica y se vuelve a asignar
        var tracker = _orbit.TrackerSettings;
        tracker.PositionDamping = Vector3.Lerp(_baseDamping, Vector3.one * _flyDamping, _blend);
        _orbit.TrackerSettings = tracker;
    }
}