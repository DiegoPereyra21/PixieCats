using UnityEngine;

public class PlayerWings : MonoBehaviour
{
    [SerializeField] private PlayerFlight _flight;
    [SerializeField] private PlayerAnimator _animator;
    [SerializeField] private Transform _leftWing;
    [SerializeField] private Transform _rightWing;

    [Header("Aleteo")]
    [SerializeField] private Vector3 _axis = Vector3.forward; // eje sobre el que giran las alas
    [SerializeField] private float _amplitude = 35f;          // grados hacia cada lado
    [SerializeField] private float _frequency = 4f;           // aleteos por segundo a velocidad 1
    [SerializeField] private float _restAngle = 0f;           // posición cuando no vuela
    [SerializeField] private float _blendSmoothTime = 0.15f;

    private Quaternion _leftRest;
    private Quaternion _rightRest;
    private float _phase;
    private float _blend;
    private float _blendVelocity;

    private void Awake()
    {
        _leftRest = _leftWing.localRotation;
        _rightRest = _rightWing.localRotation;
    }

    private void LateUpdate()
    {
        bool flying = _flight.IsFlying;
        _blend = Mathf.SmoothDamp(_blend, flying ? 1f : 0f, ref _blendVelocity, _blendSmoothTime);

        if (flying) _phase = Mathf.Repeat(_phase + Time.deltaTime * _frequency * _animator.WingSpeed * Mathf.PI * 2f, Mathf.PI * 2f);

        float angle = _restAngle + Mathf.Sin(_phase) * _amplitude * _blend;
        _leftWing.localRotation = _leftRest * Quaternion.AngleAxis(-angle, _axis);
        _rightWing.localRotation = _rightRest * Quaternion.AngleAxis(angle, _axis);
    }
}