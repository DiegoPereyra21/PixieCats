
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;

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
    [Header("Paginado")]
    [SerializeField, Min(20)] private int maxCharsPerPage = 110;

    private readonly List<string> pages = new List<string>();
    private int currentPageIndex;

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

        SplitIntoPages(line.Text);
        currentPageIndex = 0;

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


        ShowPage();
    }

    private void AdvanceDialogue()
    {
        if (!isDialogueOpen) return;

        if (currentPageIndex < pages.Count - 1)
        {
            currentPageIndex++;
            ShowPage();
            return;
        }

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

    private void ShowPage()
    {
        dialogueText.text = pages[currentPageIndex];

        bool lastPage = currentPageIndex == pages.Count - 1;
        bool lastLine = currentLineIndex == currentDialogue.Lines.Length - 1;

        nextButton.text = lastPage && lastLine ? " [E] Terminar" : " [E] Continuar";
    }

    // corta en fin de oración si puede, si no en espacio, sin pasar de maxCharsPerPage
    private void SplitIntoPages(string text)
    {
        pages.Clear();
        text = string.IsNullOrEmpty(text) ? string.Empty : text.Trim();

        while (text.Length > maxCharsPerPage)
        {
            int cut = text.LastIndexOf(' ', maxCharsPerPage);
            int sentence = text.LastIndexOfAny(new[] { '.', '!', '?', '…' }, maxCharsPerPage - 1);

            if (sentence > maxCharsPerPage * 0.5f) cut = sentence + 1;
            else if (cut <= 0) cut = maxCharsPerPage;

            pages.Add(text.Substring(0, cut).Trim());
            text = text.Substring(cut).Trim();
        }

        pages.Add(text);
    }
}
