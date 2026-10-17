public interface IInteractable
{
    bool CanInteract { get; }
    void Interact(PlayerInteractor interactor);
    void SetHighlighted(bool highlighted);
}