using Unity.Cinemachine;
using UnityEngine;

public class CameraFovController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private Rigidbody _target;

    [SerializeField] private float _baseFov = 60f;
    [SerializeField] private float _extraFov = 12f;
    [SerializeField] private float _referenceSpeed = 27f; // velocidad con la que llega al FOV máximo
    [SerializeField] private float _smoothTime = 0.25f;

    private float _currentFov;
    private float _fovVelocity;

    private void Start() => _currentFov = _baseFov;

    private void LateUpdate()
    {
        Vector3 velocity = _target.linearVelocity;
        velocity.y = 0f;

        // al cuadrado: casi no se nota caminando y se siente más volando
        float t = Mathf.Clamp01(velocity.magnitude / _referenceSpeed);
        float targetFov = _baseFov + _extraFov * t * t;

        _currentFov = Mathf.SmoothDamp(_currentFov, targetFov, ref _fovVelocity, _smoothTime);

        LensSettings lens = _camera.Lens;
        lens.FieldOfView = _currentFov;
        _camera.Lens = lens;
    }
}