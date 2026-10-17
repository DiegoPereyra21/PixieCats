using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    private const float JumpGroundIgnoreTime = 0.15f; // tras saltar, el sensor sigue tocando el suelo unos frames

    [SerializeField] private PlayerInputReader _input;
    [SerializeField] private PlayerFlight _flight;
    [SerializeField] private Transform _cameraTransform;

    [Header("Movimiento")]
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float _sprintSpeed = 12f;
    [SerializeField] private float _acceleration = 50f;
    [SerializeField] private float _rotationSpeed = 20f;

    [Header("Salto")]
    [SerializeField] private float _jumpHeight = 3f;
    [SerializeField] private float _coyoteTime = 0.1f;
    [SerializeField] private float _jumpBufferTime = 0.15f;
    [SerializeField] private float _bufferGroundDistance = 0.6f; // en esta distancia al suelo, cayendo, Space es salto y no vuelo

    [Header("Caída")]
    [SerializeField] private float _fallGravityMultiplier = 1.8f;
    [SerializeField] private float _maxFallSpeed = 30f;

    [Header("Aterrizaje")]
    [SerializeField] private float _minLandingImpact = 2f;
    [SerializeField] private float _landingSlideTime = 0.2f;
    [SerializeField] private float _landingGrip = 0.3f; // fracción de la aceleración mientras patina

    [Header("Suelo")]
    [SerializeField] private LayerMask _groundMask = ~0; // excluir la layer Player
    [SerializeField] private float _groundCheckRadius = 0.3f;
    [SerializeField] private float _groundCheckDistance = 0.1f;

    public bool IsGrounded { get; private set; }

    // true si un Space ahora debe ser un salto (suelo, coyote o pre-aterrizaje) y no el inicio del vuelo
    public bool PrefersGroundJump => IsGrounded || CanGroundJump || IsFallingNearGround();

    // velocidad vertical de impacto (positiva) al aterrizar
    public event Action<float> Landed;

    private bool CanGroundJump => _lastGroundedTime > _lastJumpTime && Time.time - _lastGroundedTime <= _coyoteTime;

    private Rigidbody _rb;
    private float _lastGroundedTime = float.NegativeInfinity;
    private float _lastJumpTime = float.NegativeInfinity;
    private float _fallSpeed;
    private float _slideUntil;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_cameraTransform == null) _cameraTransform = Camera.main.transform;
    }

    private void FixedUpdate()
    {
        CheckGround();
        Rotate();

        // mientras vuela, PlayerFlight controla el movimiento
        if (_flight != null && _flight.IsFlying)
        {
            _input.ConsumeJump(); // evita un salto fantasma al aterrizar
            return;
        }

        float acceleration = Time.time < _slideUntil ? _acceleration * _landingGrip : _acceleration;
        MoveHorizontal(_input.Sprint ? _sprintSpeed : _walkSpeed, acceleration);

        Jump();
        ApplyFallGravity();
    }

    // Mueve en el plano horizontal relativo a la cámara, sin tocar la velocidad vertical
    public void MoveHorizontal(float speed, float acceleration)
    {
        // dirección relativa a la cámara, sin componente vertical
        Vector3 camForward = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized;
        Vector3 direction = Vector3.ClampMagnitude(camForward * _input.Move.y + camRight * _input.Move.x, 1f);

        Vector3 targetVelocity = direction * speed;

        Vector3 velocity = _rb.linearVelocity;
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        horizontal = Vector3.MoveTowards(horizontal, targetVelocity, acceleration * Time.fixedDeltaTime);

        _rb.linearVelocity = new Vector3(horizontal.x, velocity.y, horizontal.z);
    }

    private void CheckGround()
    {
        bool wasGrounded = IsGrounded;

        // esfera en los pies (el pivot del player debe estar en el suelo)
        Vector3 origin = transform.position + Vector3.up * (_groundCheckRadius - _groundCheckDistance);
        IsGrounded = Physics.CheckSphere(origin, _groundCheckRadius, _groundMask, QueryTriggerInteraction.Ignore);

        if (IsGrounded)
        {
            // el coyote solo se renueva si no acaba de saltar
            if (Time.time - _lastJumpTime > JumpGroundIgnoreTime) _lastGroundedTime = Time.time;

            if (!wasGrounded) OnLanded(-_fallSpeed);
            _fallSpeed = 0f;
        }
        else
        {
            // la velocidad al chocar ya se perdió en el paso de física, por eso se guarda la mínima en el aire
            _fallSpeed = Mathf.Min(_fallSpeed, _rb.linearVelocity.y);
        }
    }

    private void OnLanded(float impact)
    {
        if (impact < _minLandingImpact) return;

        Vector3 velocity = _rb.linearVelocity;
        float horizontalSpeed = new Vector2(velocity.x, velocity.z).magnitude;

        // solo patina si llega más rápido que el sprint (vuelo)
        if (horizontalSpeed > _sprintSpeed + 0.5f) _slideUntil = Time.time + _landingSlideTime;

        Landed?.Invoke(impact);
    }

    private void Rotate()
    {
        // siempre mira hacia donde apunta la cámara
        Quaternion target = Quaternion.Euler(0f, _cameraTransform.eulerAngles.y, 0f);
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, target, _rotationSpeed * Time.fixedDeltaTime));
    }

    private void Jump()
    {
        if (!_input.JumpBuffered(_jumpBufferTime) || !CanGroundJump) return;

        // v = sqrt(2 * g * h)
        float jumpVelocity = Mathf.Sqrt(_jumpHeight * -2f * Physics.gravity.y);
        Vector3 velocity = _rb.linearVelocity;
        _rb.linearVelocity = new Vector3(velocity.x, jumpVelocity, velocity.z);

        _lastJumpTime = Time.time;
        _input.ConsumeJump();
    }

    private void ApplyFallGravity()
    {
        Vector3 velocity = _rb.linearVelocity;
        if (velocity.y >= 0f || IsGrounded) return;

        // gravedad extra solo al bajar
        _rb.AddForce(Physics.gravity * (_fallGravityMultiplier - 1f), ForceMode.Acceleration);

        if (velocity.y < -_maxFallSpeed)
            _rb.linearVelocity = new Vector3(velocity.x, -_maxFallSpeed, velocity.z);
    }

    private bool IsFallingNearGround()
    {
        if (_rb.linearVelocity.y >= 0f) return false;

        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, _bufferGroundDistance + 0.1f, _groundMask, QueryTriggerInteraction.Ignore);
    }
}