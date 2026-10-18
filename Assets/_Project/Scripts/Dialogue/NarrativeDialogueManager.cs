
using UnityEngine;
using UnityEngine.UIElements;
using System;

public class NarrativeDialogueManager : MonoBehaviour
{
    public static NarrativeDialogueManager Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private PlayerInputReader playerInput;

    private VisualElement dialogueRoot;
    private VisualElement portraitContainer;
    private Image characterPortrait;
    private Label characterName;
    private Label dialogueText;
    private Button nextButton;

    private NarrativeDialogueData currentDialogue;
    private int currentLineIndex;
    private bool isDialogueOpen;

    public bool IsDialogueOpen => isDialogueOpen;

    public event Action<NarrativeDialogueData> DialogueEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Hay mas de un NarrativeDialogueManager en la escena.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("NarrativeDialogueManager necesita un UIDocument.");
            enabled = false;
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;

        dialogueRoot = root.Q<VisualElement>("NarrativeDialogueRoot");
        portraitContainer = root.Q<VisualElement>("PortraitContainer");
        characterPortrait = root.Q<Image>("CharacterPortrait");
        characterName = root.Q<Label>("CharacterName");
        dialogueText = root.Q<Label>("DialogueText");
        nextButton = root.Q<Button>("NextButton");

        if (characterPortrait == null)
            Debug.LogWarning("NarrativeDialogueManager: 'CharacterPortrait' no existe o no es un Image en el UXML.");

        if (dialogueRoot == null || characterName == null || dialogueText == null || nextButton == null)
        {
            Debug.LogError("Faltan elementos en NarrativeDialogueHUD. Revisa los nombres del UXML.");

            enabled = false;
            return;
        }

        nextButton.clicked += AdvanceDialogue;
        dialogueRoot.style.display = DisplayStyle.None;
    }

    private void OnEnable()
    {
        if (playerInput != null && isDialogueOpen)
            playerInput.InteractPressed += AdvanceDialogue;
    }

    private void OnDisable()
    {
        if (playerInput != null)
            playerInput.InteractPressed -= AdvanceDialogue;

        if (nextButton != null)
            nextButton.clicked -= AdvanceDialogue;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool StartDialogue(NarrativeDialogueData dialogue)
    {
        if (isDialogueOpen || dialogue == null || dialogue.Lines == null || dialogue.Lines.Length == 0)
        {
            return false;
        }

        if (playerInput == null)
        {
            Debug.LogError("Asigna PlayerInputReader en NarrativeDialogueManager.");
            return false;
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueOpen = true;

        playerInput.InteractPressed += AdvanceDialogue;

        dialogueRoot.style.display = DisplayStyle.Flex;
        ShowCurrentLine();

        return true;
    }

    private void ShowCurrentLine()
    {
        NarrativeDialogueLine line = currentDialogue.Lines[currentLineIndex];

        dialogueText.text = line.Text;

        DialogueCharacterData character = line.Character;

        if (character != null)
        {
            characterName.text = character.CharacterName;

            Sprite portrait = character.GetPortrait(line.Emotion);

            if (portrait == null)
                Debug.Log($"Dialogo línea {currentLineIndex}: character={(line.Character != null ? line.Character.name : "NULL")}, emotion={line.Emotion}", currentDialogue);

            if (characterPortrait != null)
                characterPortrait.sprite = portrait;

            if (portraitContainer != null)
                portraitContainer.style.display = portrait != null ? DisplayStyle.Flex : DisplayStyle.None;
        }
        else
        {
            characterName.text = string.Empty;

            if (portraitContainer != null)
                portraitContainer.style.display = DisplayStyle.None;
        }

        nextButton.text = currentLineIndex == currentDialogue.Lines.Length - 1 ? " [E] Terminar" : " [E] Continuar";
    }

    private void AdvanceDialogue()
    {
        if (!isDialogueOpen)
            return;

        currentLineIndex++;

        if (currentLineIndex >= currentDialogue.Lines.Length)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    private void EndDialogue()
    {
        if (playerInput != null)
            playerInput.InteractPressed -= AdvanceDialogue;

        NarrativeDialogueData finished = currentDialogue;

        isDialogueOpen = false;
        currentDialogue = null;

        dialogueRoot.style.display = DisplayStyle.None;

        DialogueEnded?.Invoke(finished);
    }
}
