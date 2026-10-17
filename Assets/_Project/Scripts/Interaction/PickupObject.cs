using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PickupObject : MonoBehaviour, IInteractable
{
    [SerializeField] private float _weight = 1f;
    [SerializeField] private float _repickDelay = 0.75f;

    public float Weight => _weight;
    public bool CanInteract => Time.time >= _blockedUntil;

    private Rigidbody _rb;
    private float _blockedUntil;

    private void Awake() => _rb = GetComponent<Rigidbody>();

    public void Interact(PlayerInteractor interactor) => interactor.Carrier.Store(this);

    public void OnStored()
    {
        // desactivado = sin render ni colisiones mientras está en la bolsa
        gameObject.SetActive(false);
    }

    public void OnDropped(Vector3 position, Vector3 velocity)
    {
        transform.position = position;
        gameObject.SetActive(true);
        _rb.linearVelocity = velocity;
        _rb.angularVelocity = Vector3.zero;
        _blockedUntil = Time.time + _repickDelay;
    }
}