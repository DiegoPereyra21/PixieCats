using UnityEngine;

[RequireComponent(typeof(InteractionHighlight))]
public class EdibleMushroom : MonoBehaviour, IInteractable
{
    [SerializeField, Min(0.1f)] private float _eatTime = 2f;
    [SerializeField, Range(0f, 1f)] private float _healPercent = 0.25f;
    [SerializeField, Range(0f, 1f)] private float _eatenScale = 0.3f; // tamaño justo antes de desaparecer

    [Header("Eventos (opcionales)")]
    [SerializeField] private VoidEventChannelSO _onEaten;

    public float EatTime => _eatTime;
    public float HealPercent => _healPercent;
    public bool CanInteract => true;
    public bool IsBlocked => false;

    private InteractionHighlight _highlight;
    private Vector3 _originalScale;

    private void Awake()
    {
        _highlight = GetComponent<InteractionHighlight>();
        _originalScale = transform.localScale;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (interactor.Eater != null) interactor.Eater.StartEating(this);
    }

    public void SetHighlighted(bool highlighted) => _highlight.Set(highlighted);

    // 0 = entero, 1 = comido; se achica mientras se come
    public void SetEatProgress(float progress) => transform.localScale = _originalScale * Mathf.Lerp(1f, _eatenScale, progress);

    public void Consume()
    {
        SetHighlighted(false);
        if (_onEaten != null) _onEaten.Raise();
        gameObject.SetActive(false);
    }
}