using UnityEngine;

[RequireComponent(typeof(InteractionHighlight))]
public class MushroomPortal : MonoBehaviour, IInteractable
{
    [SerializeField] private MissionManager _manager;
    [SerializeField] private SceneLoader _sceneLoader;

    public bool CanInteract => _chainFinished && !_used;
    public bool IsBlocked => false;

    private InteractionHighlight _highlight;
    private bool _chainFinished;
    private bool _used;

    private void Awake() => _highlight = GetComponent<InteractionHighlight>();

    private void OnEnable() => _manager.ChainFinished += OnChainFinished;

    private void OnDisable() => _manager.ChainFinished -= OnChainFinished;

    private void OnChainFinished() => _chainFinished = true;

    public void Interact(PlayerInteractor interactor)
    {
        if (!CanInteract) return;
        _used = true; // evita cargar la transición dos veces

        // el calendario avanza a la noche siguiente; GameProgressManager sobrevive a la recarga
        if (GameProgressManager.Instance != null) GameProgressManager.Instance.ReturnThroughMushroomCircle();

        _sceneLoader.LoadTransition();
    }

    public void SetHighlighted(bool highlighted) => _highlight.Set(highlighted);
}