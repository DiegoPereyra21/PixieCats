public interface IInteractable
{
    bool CanInteract { get; }
    bool IsBlocked { get; } // presente pero bloqueado: E no hace nada en vez de tirar
    void Interact(PlayerInteractor interactor);
    void SetHighlighted(bool highlighted);
}