using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerFlight : MonoBehaviour
{
    [SerializeField] private PlayerInputReader _input;
    [SerializeField] private PlayerMovement _movement;

    [Header("Horizontal")]
    [SerializeField] private float _flySpeed = 18f;
    [SerializeField] private float _flyAcceleration = 10f;
    [SerializeField] private float _sprintMultiplier = 1.5f;

    [Header("Vertical")]
    [SerializeField] private float _flapAscendSpeed = 6f;     // subida al mantener Space
    [SerializeField] private float _glideDescendSpeed = 3f;   // bajada al soltar Space
    [SerializeField] private float _maxDescendSpeed = 12f;    // tope de caída con peso
    [SerializeField] private float _verticalAcceleration = 25f;
    [SerializeField] private float _maxHeight = 20f;          // altura máxima sobre el suelo de abajo
    [SerializeField] private LayerMask _groundMask = ~0;      // excluir la layer Player
    [SerializeField] private float _groundProbeDistance = 200f;
    [SerializeField] private float _ceilingSoftZone = 2f;     // distancia donde la subida se frena
    [SerializeField] private float _brakeDescendSpeed = 25f;       // velocidad de caída al frenar con S
    [SerializeField] private float _brakeVerticalAcceleration = 80f; // qué tan rápido llega a esa caída

    [Header("Despegue")]
    [SerializeField] private float _takeoffImpulse = 4f;      // velocidad vertical mínima al activar el vuelo

    [Header("Aleteo")]
    [SerializeField] private float _bobAmplitude = 2f;
    [SerializeField] private float _bobFrequency = 6f;

    [Header("Peso")]
    [SerializeField] private float _carriedWeight;
    [SerializeField] private float _weightInfluence = 0.1f;
    [SerializeField] private float _weightSmoothTime = 0.4f;  // tiempo en que el peso efectivo alcanza al real

    public bool IsFlying { get; private set; }
    public float CurrentWeight => _currentWeight;
    public float WeightFactor => 1f + _currentWeight * _weightInfluence;
    private float _lastGroundY;
    public event Action TookOff;

    private Rigidbody _rb;
    private float _bobPhase;
    private float _currentWeight;
    private float _weightVelocity;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _currentWeight = _carriedWeight;
    }

    private void OnEnable() => _input.JumpPressed += OnJumpPressed;

    private void OnDisable()
    {
        _input.JumpPressed -= OnJumpPressed;
        if (IsFlying) StopFlight();
    }

    // Cambia el peso que lleva el hada (0 = sin carga)
    public void SetCarriedWeight(float weight) => _carriedWeight = Mathf.Max(0f, weight);

    private void OnJumpPressed()
    {
        // en el suelo, con coyote o cayendo cerca del suelo, Space es un salto y no el inicio del vuelo
        if (IsFlying || _movement.PrefersGroundJump) return;

        StartFlight();
        _input.ConsumeJump(); // este Space ya se usó para volar
    }

    private void FixedUpdate()
    {
        // el peso efectivo persigue al real, así recoger o soltar no cambia la velocidad de golpe
        _currentWeight = Mathf.SmoothDamp(_currentWeight, _carriedWeight, ref _weightVelocity, _weightSmoothTime, Mathf.Infinity, Time.fixedDeltaTime);

        if (!IsFlying) return;

        // aterrizar corta el vuelo
        if (_movement.IsGrounded)
        {
            StopFlight();
            return;
        }

        float weightFactor = 1f + _currentWeight * _weightInfluence;
        float ceiling = GetGroundY() + _maxHeight;

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
            _movement.MoveHorizontal(speed / (1f + _currentWeight * _weightInfluence * 0.5f), _flyAcceleration);
        }
        bool braking = _input.BrakeHeld;

        float targetY;
        if (braking)
        {
            // S: frena en horizontal y se deja caer rápido (ignora Space)
            targetY = -_brakeDescendSpeed;
        }
        else if (_input.JumpHeld)
        {
            // cerca del techo la subida se frena gradualmente
            float ceilingFactor = Mathf.Clamp01((ceiling - _rb.position.y) / _ceilingSoftZone);
            targetY = (_flapAscendSpeed / weightFactor) * ceilingFactor;
        }
        else
        {
            targetY = -Mathf.Min(_glideDescendSpeed * weightFactor, _maxDescendSpeed);
        }

        // oscilación constante; con más peso aletea más lento (fase acumulada para evitar saltos al cambiar el peso)
        _bobPhase += (_bobFrequency / weightFactor) * Time.fixedDeltaTime;
        if (!braking) targetY += Mathf.Sin(_bobPhase) * _bobAmplitude;

        if (_rb.position.y >= ceiling) targetY = Mathf.Min(targetY, 0f);

        float acceleration = braking ? _brakeVerticalAcceleration : _verticalAcceleration;

        Vector3 velocity = _rb.linearVelocity;
        velocity.y = Mathf.MoveTowards(velocity.y, targetY, acceleration * Time.fixedDeltaTime);
        _rb.linearVelocity = velocity;
    }

    private void StartFlight()
    {
        _lastGroundY = _rb.position.y;
        IsFlying = true;
        _rb.useGravity = false; // la velocidad vertical la controla este script
        _bobPhase = 0f;

        // el primer aleteo da un empujón hacia arriba, más débil con peso
        float weightFactor = 1f + _currentWeight * _weightInfluence;
        Vector3 velocity = _rb.linearVelocity;
        velocity.y = Mathf.Max(velocity.y, _takeoffImpulse / weightFactor);
        _rb.linearVelocity = velocity;

        TookOff?.Invoke();
    }

    private void StopFlight()
    {
        IsFlying = false;
        _rb.useGravity = true;
    }

    // Y del suelo bajo el hada; si no encuentra nada (vacío) conserva el último valor conocido
    private float GetGroundY()
    {
        Vector3 origin = _rb.position + Vector3.up * 0.5f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _groundProbeDistance, _groundMask, QueryTriggerInteraction.Ignore))
            _lastGroundY = hit.point.y;

        return _lastGroundY;
    }
}