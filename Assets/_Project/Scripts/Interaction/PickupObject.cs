using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PickupObject : MonoBehaviour, IInteractable
{
    [SerializeField] private float _weight = 1f;
    [SerializeField] private float _repickDelay = 0.75f;
    [SerializeField] private float _absorbTime = 0.2f;

    public float Weight => _weight;
    public bool CanInteract => !_isAbsorbing && Time.time >= _blockedUntil;

    private Rigidbody _rb;
    private Collider[] _colliders;
    private Vector3 _originalScale;
    private float _blockedUntil;
    private bool _isAbsorbing;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _colliders = GetComponentsInChildren<Collider>();
        _originalScale = transform.localScale;
    }

    public void Interact(PlayerInteractor interactor) => interactor.Carrier.Store(this);

    /// Vuela hacia el destino achicándose; al llegar avisa y se desactiva.
    public void AbsorbInto(Transform target, Action onArrived)
    {
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
        transform.localScale = _originalScale;
        gameObject.SetActive(true);

        _rb.isKinematic = false;
        foreach (Collider c in _colliders) c.enabled = true;

        _rb.linearVelocity = velocity;
        _rb.angularVelocity = Vector3.zero;
        _blockedUntil = Time.time + _repickDelay;
    }
}