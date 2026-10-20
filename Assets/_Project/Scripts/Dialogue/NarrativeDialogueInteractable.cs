using UnityEngine;
using Unity.Cinemachine;


public class NarrativeDialogueInteractable : MonoBehaviour, IInteractable
{
    [Header("Dialogo")]
    [SerializeField] private NarrativeDialogueData dialogue;

    [Header("Inicio automatico")]
    [SerializeField] private bool playOnStart = false; // dialogo interno: se abre solo al cargar la escena

    [Header("Resaltado")]
    [SerializeField] private GameObject highlightVisual;

    [SerializeField] private CinemachineCamera dialogueCamera;
    private bool autoPlayed;

    public bool IsBlocked => !CanInteract;

    // un dialogo automatico no se puede repetir con E
    public bool CanInteract => !(playOnStart && autoPlayed) && dialogue != null && NarrativeDialogueManager.Instance != null && !NarrativeDialogueManager.Instance.IsDialogueOpen;

    private void Start()
    {
        if (!playOnStart || dialogue == null || NarrativeDialogueManager.Instance == null)
            return;

        autoPlayed = NarrativeDialogueManager.Instance.StartDialogue(dialogue, false, dialogueCamera);
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (!CanInteract)
            return;

        NarrativeDialogueManager.Instance.StartDialogue(dialogue, false, dialogueCamera);
    }

    public void SetHighlighted(bool highlighted)
    {
        if (highlightVisual != null)
            highlightVisual.SetActive(highlighted);
    }
}