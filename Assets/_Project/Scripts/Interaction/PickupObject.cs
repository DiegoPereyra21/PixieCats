using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PickupObject : MonoBehaviour, IInteractable
{
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    [SerializeField] private float _weight = 1f;
    [SerializeField] private float _repickDelay = 0.75f;
    [SerializeField] private float _absorbTime = 0.2f;

    [Header("Al tirarlo")]
    [SerializeField] private float _popTime = 0.25f;  // duración de la aparición (escala 0 a 1)
    [SerializeField] private float _dropSpin = 6f;    // giro inicial (rad/s), menor cuanto más pesa

    [Header("Resaltado")]
    [SerializeField, ColorUsage(false, true)] private Color _highlightColor = new Color(0.8f, 0.6f, 0.15f);

    public float Weight => _weight;
    public bool CanInteract => !_isAbsorbing && Time.time >= _blockedUntil;

    private Rigidbody _rb;
    private Collider[] _colliders;
    private Material[] _materials;
    private Vector3 _originalScale;
    private float _blockedUntil;
    private bool _isAbsorbing;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _colliders = GetComponentsInChildren<Collider>();
        _originalScale = transform.localScale;

        // .materials crea instancias propias, así el resaltado no afecta a otros objetos
        List<Material> materials = new List<Material>();
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) materials.AddRange(r.materials);
        _materials = materials.ToArray();
    }

    public void Interact(PlayerInteractor interactor) => interactor.Carrier.Store(this);

    public void SetHighlighted(bool highlighted)
    {
        foreach (Material m in _materials)
        {
            if (!m.HasProperty(EmissionColorId)) continue;

            // URP Lit ignora la emisión si el keyword está apagado
            if (highlighted) m.EnableKeyword("_EMISSION");
            m.SetColor(EmissionColorId, highlighted ? _highlightColor : Color.black);
        }
    }

    // Vuela hacia el destino achicándose; al llegar avisa y se desactiva
    public void AbsorbInto(Transform target, Action onArrived)
    {
        SetHighlighted(false);
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