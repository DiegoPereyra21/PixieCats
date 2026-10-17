using UnityEngine;

public class PlayerVisualTilt : MonoBehaviour
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private PlayerFlight _flight;
    [SerializeField] private Transform _visual;

    [Header("Inclinación")]
    [SerializeField] private float _maxPitch = 25f;       // hacia adelante / atrás
    [SerializeField] private float _maxRoll = 20f;        // lateral
    [SerializeField] private float _referenceSpeed = 27f; // velocidad con la que llega al máximo (vuelo con sprint)
    [SerializeField] private float _smoothTime = 0.15f;

    [Header("Peso")]
    [SerializeField] private float _weightDamping = 0.05f; // cada punto de peso inclina menos y más lento

    private Vector2 _angles;
    private Vector2 _anglesVelocity;

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

        _angles = Vector2.SmoothDamp(_angles, target, ref _anglesVelocity, _smoothTime * weightScale);
        _visual.localRotation = Quaternion.Euler(_angles.x, 0f, _angles.y);
    }
}