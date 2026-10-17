using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerFlight : MonoBehaviour
{
    [SerializeField] private PlayerInputReader _input;
    [SerializeField] private PlayerMovement _movement;

    [Header("Horizontal")]
    [SerializeField] private float _flySpeed = 25f;
    [SerializeField] private float _flyAcceleration = 30f;
    [SerializeField] private float _sprintMultiplier = 1.5f;

    [Header("Vertical")]
    [SerializeField] private float _flapAscendSpeed = 6f;     // subida al mantener Space
    [SerializeField] private float _glideDescendSpeed = 2.5f; // bajada al soltar Space
    [SerializeField] private float _verticalAcceleration = 12f;
    [SerializeField] private float _maxDescendSpeed = 12f; //para q si pesa demasiado pueda seguir volando
    [SerializeField] private float _maxHeight = 20f;          // Y de mundo
    [SerializeField] private float _ceilingSoftZone = 2f;     // distancia donde la subida se frena

    [Header("Aleteo")]
    [SerializeField] private float _bobAmplitude = 0.8f;
    [SerializeField] private float _bobFrequency = 3f;

    [Header("Peso")]
    [SerializeField] private float _carriedWeight;
    [SerializeField] private float _weightInfluence = 0.15f;

    public bool IsFlying { get; private set; }

    private Rigidbody _rb;
    private float _bobPhase;

    private void Awake() => _rb = GetComponent<Rigidbody>();

    private void OnEnable() => _input.JumpPressed += OnJumpPressed;

    private void OnDisable()
    {
        _input.JumpPressed -= OnJumpPressed;
        if (IsFlying) StopFlight();
    }

    /// Cambia el peso que lleva el hada (0 = sin carga).
    public void SetCarriedWeight(float weight) => _carriedWeight = Mathf.Max(0f, weight);

    private void OnJumpPressed()
    {
        if (!IsFlying && !_movement.IsGrounded) StartFlight();
    }

    private void FixedUpdate()
    {
        if (!IsFlying) return;

        // aterrizar corta el vuelo
        if (_movement.IsGrounded)
        {
            StopFlight();
            return;
        }

        float weightFactor = 1f + _carriedWeight * _weightInfluence;

        if (_input.BrakeHeld)
        {
            // S frena de golpe en horizontal, la vertical sigue
            Vector3 current = _rb.linearVelocity;
            _rb.linearVelocity = new Vector3(0f, current.y, 0f);
        }
        else
        {
            float speed = _input.Sprint ? _flySpeed * _sprintMultiplier : _flySpeed;

            // el peso hace más lento el avance (la mitad de influencia que en vertical)
            _movement.MoveHorizontal(speed / (1f + _carriedWeight * _weightInfluence * 0.5f), _flyAcceleration);
        }

        float targetY;
        if (_input.JumpHeld)
        {
            // cerca del techo la subida se frena gradualmente
            float ceilingFactor = Mathf.Clamp01((_maxHeight - _rb.position.y) / _ceilingSoftZone);
            targetY = (_flapAscendSpeed / weightFactor) * ceilingFactor;
        }
        else
        {
            targetY = -Mathf.Min(_glideDescendSpeed * weightFactor, _maxDescendSpeed);
        }

        // oscilación constante; con más peso aletea más lento (fase acumulada para evitar saltos al cambiar el peso)
        _bobPhase += (_bobFrequency / weightFactor) * Time.fixedDeltaTime;
        targetY += Mathf.Sin(_bobPhase) * _bobAmplitude;

        if (_rb.position.y >= _maxHeight) targetY = Mathf.Min(targetY, 0f);

        Vector3 velocity = _rb.linearVelocity;
        velocity.y = Mathf.MoveTowards(velocity.y, targetY, _verticalAcceleration * Time.fixedDeltaTime);
        _rb.linearVelocity = velocity;
    }

    private void StartFlight()
    {
        IsFlying = true;
        _rb.useGravity = false; // la velocidad vertical la controla este script
        _bobPhase = 0f;
    }

    private void StopFlight()
    {
        IsFlying = false;
        _rb.useGravity = true;
    }
}