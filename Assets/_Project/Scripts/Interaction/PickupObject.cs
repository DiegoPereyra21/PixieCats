using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(InteractionHighlight))]
public class PickupObject : MonoBehaviour, IInteractable
{
    [SerializeField] private float _weight = 1f;
    [SerializeField] private float _repickDelay = 0.75f;
    [SerializeField] private float _absorbTime = 0.2f;

    [Header("Al tirarlo")]
    [SerializeField] private float _popTime = 0.25f;  // duración de la aparición (escala 0 a 1)
    [SerializeField] private float _dropSpin = 6f;    // giro inicial (rad/s), menor cuanto más pesa

    public float Weight => _weight;
    public bool IsHeld { get; private set; }     // desde que lo agarran hasta que lo tiran
    public bool IsPickable { get; set; } = true; // lo apaga el sistema de misiones al llegar al cupo
    public bool CanInteract => IsPickable && !_isAbsorbing && Time.time >= _blockedUntil;
    public bool IsBlocked => !IsPickable && !IsHeld; // fuera de cupo de la misión

    public event Action<PickupObject> Grabbed;
    public event Action<PickupObject> Dropped;

    private Rigidbody _rb;
    private Collider[] _colliders;
    private InteractionHighlight _highlight;
    private Vector3 _originalScale;
    private float _blockedUntil;
    private bool _isAbsorbing;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _colliders = GetComponentsInChildren<Collider>();
        _originalScale = transform.localScale;

        // en objetos viejos de la escena el componente puede faltar
        _highlight = GetComponent<InteractionHighlight>();
        if (_highlight == null) _highlight = gameObject.AddComponent<InteractionHighlight>();
    }

    public void Interact(PlayerInteractor interactor) => interactor.Carrier.Store(this);

    public void SetHighlighted(bool highlighted) => _highlight.Set(highlighted);

    // Vuela hacia el destino achicándose; al llegar avisa y se desactiva
    public void AbsorbInto(Transform target, Action onArrived)
    {
        IsHeld = true;
        SetHighlighted(false);
        Grabbed?.Invoke(this);

        StartCoroutine(AbsorbRoutine(target, onArrived));
    }

    private IEnumerator AbsorbRoutine(Transform target, Action onArrived)
    {
        _isAbsorbing = true;

        // sin física ni colisiones mientras vuela a la bolsa
        _rb.isKinematic = true;
        foreach (Collider c in _colliders) c.enabled = false;

        Vector3 startPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < _absorbTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _absorbTime);
            float eased = t * t; // acelera hacia el final

            // el destino se lee cada frame porque la bolsa se mueve
            transform.position = Vector3.Lerp(startPosition, target.position, eased);
            transform.localScale = Vector3.Lerp(_originalScale, Vector3.zero, eased);
            yield return null;
        }

        _isAbsorbing = false;
        onArrived?.Invoke(); // antes de desactivar, porque desactivar corta la corrutina
        gameObject.SetActive(false);
    }

    public void OnDropped(Vector3 position, Vector3 velocity)
    {
        transform.position = position;
        gameObject.SetActive(true);

        _rb.isKinematic = false;
        foreach (Collider c in _colliders) c.enabled = true;

        _rb.linearVelocity = velocity;
        _rb.angularVelocity = UnityEngine.Random.onUnitSphere * (_dropSpin / (1f + _weight * 0.05f));
        _blockedUntil = Time.time + _repickDelay;

        IsHeld = false;
        Dropped?.Invoke(this);

        StartCoroutine(PopInRoutine());
    }

    private IEnumerator PopInRoutine()
    {
        float elapsed = 0f;

        while (elapsed < _popTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _popTime);

            // escala mínima > 0 para no dejar el collider degenerado
            transform.localScale = _originalScale * Mathf.Max(0.01f, EaseOutBack(t));
            yield return null;
        }

        transform.localScale = _originalScale;
    }

    // sube pasándose un poco de 1 y se asienta
    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}