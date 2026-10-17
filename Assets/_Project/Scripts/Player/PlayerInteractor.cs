using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private PlayerInputReader _input;
    [SerializeField] private PlayerCarrier _carrier;
    [SerializeField] private float _interactRadius = 2.5f;
    [SerializeField] private LayerMask _interactMask = ~0; // usar solo la layer Interactable

    public PlayerCarrier Carrier => _carrier;

    private readonly Collider[] _hits = new Collider[16];

    private void OnEnable() => _input.InteractPressed += OnInteractPressed;
    private void OnDisable() => _input.InteractPressed -= OnInteractPressed;

    private void OnInteractPressed()
    {
        IInteractable target = FindClosest();

        if (target != null) target.Interact(this);
        else _carrier.DropLast(); // sin nada en rango, suelta el último objeto
    }

    private IInteractable FindClosest()
    {
        Vector3 center = transform.position + Vector3.up; // centro del cuerpo
        int count = Physics.OverlapSphereNonAlloc(center, _interactRadius, _hits, _interactMask, QueryTriggerInteraction.Ignore);

        IInteractable closest = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            IInteractable interactable = _hits[i].GetComponentInParent<IInteractable>();
            if (interactable == null || !interactable.CanInteract) continue;

            float distance = (_hits[i].transform.position - center).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }

        return closest;
    }
}