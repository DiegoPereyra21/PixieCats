using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerInputReader _input;
    [SerializeField] private Transform _cameraTransform;

    [Header("Movimiento")]
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private float _acceleration = 60f;
    [SerializeField] private float _rotationSpeed = 15f;

    [Header("Salto")]
    [SerializeField] private float _jumpHeight = 1.5f;

    [Header("Suelo")]
    [SerializeField] private LayerMask _groundMask = ~0; // excluir la layer Player
    [SerializeField] private float _groundCheckRadius = 0.3f;
    [SerializeField] private float _groundCheckDistance = 0.1f;

    private Rigidbody _rb;
    private bool _isGrounded;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_cameraTransform == null) _cameraTransform = Camera.main.transform;
    }

    private void FixedUpdate()
    {
        CheckGround();
        Move();
        Rotate();
        Jump();
    }

    private void CheckGround()
    {
        // esfera en los pies (el pivot del player debe estar en el suelo)
        Vector3 origin = transform.position + Vector3.up * (_groundCheckRadius - _groundCheckDistance);
        _isGrounded = Physics.CheckSphere(origin, _groundCheckRadius, _groundMask, QueryTriggerInteraction.Ignore);
    }

    private void Move()
    {
        // dirección relativa a la cámara, sin componente vertical
        Vector3 camForward = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized;
        Vector3 direction = Vector3.ClampMagnitude(camForward * _input.Move.y + camRight * _input.Move.x, 1f);

        float speed = _input.Sprint ? _sprintSpeed : _walkSpeed;
        Vector3 targetVelocity = direction * speed;

        Vector3 velocity = _rb.linearVelocity;
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        horizontal = Vector3.MoveTowards(horizontal, targetVelocity, _acceleration * Time.fixedDeltaTime);

        _rb.linearVelocity = new Vector3(horizontal.x, velocity.y, horizontal.z);
    }

    private void Rotate()
    {
        // siempre mira hacia donde apunta la cámara
        Quaternion target = Quaternion.Euler(0f, _cameraTransform.eulerAngles.y, 0f);
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, target, _rotationSpeed * Time.fixedDeltaTime));
    }

    private void Jump()
    {
        if (_input.JumpRequested && _isGrounded)
        {
            // v = sqrt(2 * g * h)
            float jumpVelocity = Mathf.Sqrt(_jumpHeight * -2f * Physics.gravity.y);
            Vector3 velocity = _rb.linearVelocity;
            _rb.linearVelocity = new Vector3(velocity.x, jumpVelocity, velocity.z);
        }

        _input.ConsumeJump();
    }
}