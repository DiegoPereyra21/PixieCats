using UnityEngine;

public class PlayerVisualTilt : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private PlayerFlight _flight;
    [SerializeField] private PlayerInputReader _input;
    [SerializeField] private PlayerAnimator _animator;
    [SerializeField] private Transform _visual;

    [Header("Inclinación")]
    [SerializeField] private float _maxPitch = 25f;       // hacia adelante / atrás
    [SerializeField] private float _maxRoll = 20f;        // lateral
    [SerializeField] private float _referenceSpeed = 27f; // velocidad con la que llega al máximo (vuelo con sprint)
    [SerializeField] private float _smoothTime = 0.15f;

    [Header("Pose de vuelo")]
    [SerializeField] private float _flyPitchOffset = -45f; // endereza la pose horizontal de Flying (más negativo = más vertical)

    [Header("Freno (S)")]
    [SerializeField] private float _brakePitch = 40f;       // inclinación hacia atrás al frenar a máxima velocidad
    [SerializeField] private float _brakeFlareTime = 0.35f; // cuánto dura el gesto de freno

    [Header("Peso")]
    [SerializeField] private float _weightDamping = 0.05f; // cada punto de peso inclina menos y más lento

    private Vector2 _angles;
    private Vector2 _anglesVelocity;
    private float _lastSpeed;
    private bool _wasBraking;
    private float _flare;
    private float _flareTimer;

    private void LateUpdate()
    {
        float weightScale = 1f + _flight.CurrentWeight * _weightDamping;
        Vector2 target = Vector2.zero;

        // solo se inclina volando; al aterrizar vuelve a 0
        if (_flight.IsFlying)
        {
            // velocidad en el espacio local del hada (z = adelante, x = derecha)
            Vector3 local = transform.InverseTransformDirection(_rb.linearVelocity);

            target.x = Mathf.Clamp(local.z / _referenceSpeed, -1f, 1f) * _maxPitch / weightScale;
            target.y = Mathf.Clamp(-local.x / _referenceSpeed, -1f, 1f) * _maxRoll / weightScale; // roll hacia donde va
        }

        // la animación Flying va casi horizontal; se endereza en proporción a cuánto de esa pose se está usando
        target.x += _flyPitchOffset * _animator.FlyBlend;

        // gesto de freno: al empezar a frenar se inclina hacia atrás según la velocidad que traía
        bool braking = _flight.IsFlying && _input.BrakeHeld;
        if (braking && !_wasBraking)
        {
            _flare = Mathf.Clamp01(_lastSpeed / _referenceSpeed);
            _flareTimer = 0f;
        }
        _wasBraking = braking;

        _flareTimer += Time.deltaTime;
        float flareFade = 1f - Mathf.Clamp01(_flareTimer / _brakeFlareTime);
        target.x -= _brakePitch * _flare * flareFade / weightScale;

        // la velocidad se guarda al final: el freno la deja en 0 y hay que recordar la de antes
        Vector3 velocity = _rb.linearVelocity;
        _lastSpeed = new Vector2(velocity.x, velocity.z).magnitude;

        _angles = Vector2.SmoothDamp(_angles, target, ref _anglesVelocity, _smoothTime * weightScale);
        _visual.localRotation = Quaternion.Euler(_angles.x, 0f, _angles.y);
    }
}