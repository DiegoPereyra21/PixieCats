using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private PlayerInputReader _input;
    [SerializeField] private PlayerCarrier _carrier;
    [SerializeField] private PlayerEater _eater;
    [SerializeField] private float _interactRadius = 2.5f;
    [SerializeField] private LayerMask _interactMask = ~0; // usar solo la layer Interactable

    public PlayerCarrier Carrier => _carrier;
    public PlayerEater Eater => _eater;
    private IInteractable _highlighted;
    private readonly Collider[] _hits = new Collider[64];
    private void OnEnable() => _input.InteractPressed += OnInteractPressed;

    private void OnDisable()
    {
        _input.InteractPressed -= OnInteractPressed;
        SetHighlighted(null);
    }

    private void Update()
    {
        // el objeto que se resalta es el mismo que agarraría E
        SetHighlighted(FindClosest(out _));
    }

    private void OnInteractPressed()
    {
        // con un diálogo abierto, E solo avanza el diálogo
        if (NarrativeDialogueManager.Instance != null && NarrativeDialogueManager.Instance.IsDialogueOpen)
            return;

        IInteractable target = FindClosest(out bool blockedInRange);

        if (target != null) target.Interact(this);
        else if (!blockedInRange) _carrier.DropLast(); // sin nada en rango, suelta el último objeto
    }

    private void SetHighlighted(IInteractable target)
    {
        if (target == _highlighted) return;

        _highlighted?.SetHighlighted(false);
        _highlighted = target;
        _highlighted?.SetHighlighted(true);
    }

    private IInteractable FindClosest(out bool blockedInRange)
    {
        blockedInRange = false;

        Vector3 center = transform.position + Vector3.up; // centro del cuerpo
        int count = Physics.OverlapSphereNonAlloc(center, _interactRadius, _hits, _interactMask, QueryTriggerInteraction.Collide);

        IInteractable closest = null;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            IInteractable interactable = _hits[i].GetComponentInParent<IInteractable>();
            if (interactable == null) continue;

            if (!interactable.CanInteract)
            {
                // hay algo bloqueado cerca: E no debe tirar objetos por error
                if (interactable.IsBlocked) blockedInRange = true;
                continue;
            }

            float distance = (_hits[i].bounds.ClosestPoint(center) - center).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = interactable;
            }
        }

        return closest;
    }
}