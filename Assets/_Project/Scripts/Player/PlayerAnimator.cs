using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int MoveMultiplierHash = Animator.StringToHash("MoveMultiplier");
    private static readonly int FlyBlendHash = Animator.StringToHash("FlyBlend");
    private static readonly int WingSpeedHash = Animator.StringToHash("WingSpeed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsFlyingHash = Animator.StringToHash("IsFlying");

    [SerializeField] private Animator _animator;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private PlayerFlight _flight;
    [SerializeField] private PlayerInputReader _input;
    [SerializeField] private Animator _wingsAnimator; // el de las alas, opcional

    [Header("Caminata")]
    [SerializeField] private float _walkReferenceSpeed = 5f; // velocidad a la que la animación de caminar se ve natural
    [SerializeField] private float _minMoveMultiplier = 0.6f;
    [SerializeField] private float _maxMoveMultiplier = 2.5f;

    [Header("Vuelo")]
    [SerializeField] private float _flyingPoseSpeed = 12f;  // a esta velocidad se usa Flying entera; por debajo se mezcla con Floating
    [SerializeField] private float _wingAscendBoost = 1.5f; // aleteo más rápido al mantener Space

    [Header("Suavizado")]
    [SerializeField] private float _speedDamp = 0.1f;
    [SerializeField] private float _flyBlendSmoothTime = 0.2f;
    [SerializeField] private float _airGraceTime = 0.1f;    // evita saltar a la animación de aire en rampas o escalones

    // 0 = pose de Floating, 1 = pose de Flying (también la usa PlayerVisualTilt)
    public float FlyBlend { get; private set; }

    private float _flyBlendVelocity;
    private float _airTime;

    private void Update()
    {
        Vector3 velocity = _rb.linearVelocity;
        float horizontalSpeed = new Vector2(velocity.x, velocity.z).magnitude;
        bool isFlying = _flight.IsFlying;

        // el suelo flojo (rampas, escalones) no debe disparar la animación de aire
        if (_movement.IsGrounded) _airTime = 0f;
        else _airTime += Time.deltaTime;

        // la misma animación de caminar sirve para el sprint, solo más rápida
        float multiplier = horizontalSpeed > 0.1f
            ? Mathf.Clamp(horizontalSpeed / _walkReferenceSpeed, _minMoveMultiplier, _maxMoveMultiplier)
            : 1f;

        _animator.SetFloat(SpeedHash, horizontalSpeed, _speedDamp, Time.deltaTime);
        _animator.SetFloat(MoveMultiplierHash, multiplier);
        _animator.SetBool(IsGroundedHash, _airTime < _airGraceTime);
        _animator.SetBool(IsFlyingHash, isFlying);

        // en vuelo: casi quieta = Floating, a toda velocidad = Flying
        float targetBlend = isFlying ? Mathf.Clamp01(horizontalSpeed / _flyingPoseSpeed) : 0f;
        FlyBlend = Mathf.SmoothDamp(FlyBlend, targetBlend, ref _flyBlendVelocity, _flyBlendSmoothTime);
        _animator.SetFloat(FlyBlendHash, FlyBlend);

        // alas: más lentas con peso y más rápidas al subir (tienen su propio Animator)
        if (_wingsAnimator != null)
        {
            float wingSpeed = 1f / Mathf.Sqrt(_flight.WeightFactor);
            if (isFlying && _input.JumpHeld) wingSpeed *= _wingAscendBoost;

            _wingsAnimator.SetBool(IsFlyingHash, isFlying);
            _wingsAnimator.SetFloat(WingSpeedHash, wingSpeed);
        }
    }
}