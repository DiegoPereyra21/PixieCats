
using UnityEngine;

public class NarrativeDialogueInteractable : MonoBehaviour, IInteractable
{
    [Header("Dialogo")]
    [SerializeField] private NarrativeDialogueData dialogue;

    [Header("Resaltado")]
    [SerializeField] private GameObject highlightVisual;

    public bool CanInteract => dialogue != null && NarrativeDialogueManager.Instance != null && !NarrativeDialogueManager.Instance.IsDialogueOpen;

    public void Interact(PlayerInteractor interactor)
    {
        if (!CanInteract)
            return;

        NarrativeDialogueManager.Instance.StartDialogue(dialogue);
    }

    public void SetHighlighted(bool highlighted)
    {
        if (highlightVisual != null)
            highlightVisual.SetActive(highlighted);
    }
}
