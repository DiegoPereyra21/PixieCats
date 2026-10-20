using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NarrativeDialogueManager : MonoBehaviour
{
    public static NarrativeDialogueManager Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private PlayerInputReader playerInput;

    [Header("Paginado")]
    [SerializeField, Min(20)] private int maxCharsPerPage = 110;

    private VisualElement dialogueRoot;
    private VisualElement portraitContainer;
    private Image characterPortrait;
    private Label characterName;
    private Label dialogueText;
    private Button nextButton;
    private Button declineButton;

    private NarrativeDialogueData currentDialogue;
    private int currentLineIndex;
    private int currentPageIndex;
    private readonly List<string> pages = new List<string>();

    private bool isDialogueOpen;
    private bool askForAcceptance; // el dialogo termina con la pregunta Si / No
    private bool askingNow;        // ultima pagina con Si / No: E no avanza, hay que clicar

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
        declineButton = root.Q<Button>("DeclineButton");

        if (declineButton != null)
        {
            declineButton.SetEnabled(false); // se ve pero no se puede apretar
            declineButton.style.display = DisplayStyle.None;
        }

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
            playerInput.InteractPressed += OnInteractPressed;
    }

    private void OnDisable()
    {
        if (playerInput != null)
            playerInput.InteractPressed -= OnInteractPressed;

        if (nextButton != null)
            nextButton.clicked -= AdvanceDialogue;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public bool StartDialogue(NarrativeDialogueData dialogue, bool askAcceptance = false)
    {
        if (isDialogueOpen || dialogue == null || dialogue.Lines == null || dialogue.Lines.Length == 0)
            return false;

        if (playerInput == null)
        {
            Debug.LogError("Asigna PlayerInputReader en NarrativeDialogueManager.");
            return false;
        }

        currentDialogue = dialogue;
        currentLineIndex = 0;
        isDialogueOpen = true;
        askForAcceptance = askAcceptance;

        playerInput.InteractPressed += OnInteractPressed;

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
                Debug.Log($"Dialogo linea {currentLineIndex}: character={(line.Character != null ? line.Character.name : "NULL")}, emotion={line.Emotion}", currentDialogue);

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

    private void ShowPage()
    {
        dialogueText.text = pages[currentPageIndex];

        bool lastPage = currentPageIndex == pages.Count - 1;
        bool lastLine = currentLineIndex == currentDialogue.Lines.Length - 1;
        bool askNow = lastPage && lastLine && askForAcceptance;

        askingNow = askNow;

        if (declineButton != null)
            declineButton.style.display = askNow ? DisplayStyle.Flex : DisplayStyle.None;

        nextButton.text = askNow ? " S\u00ED" : (lastPage && lastLine ? " [E] Terminar" : " [E] Continuar");
    }

    // E no avanza en la pregunta Si / No: ahi solo se puede clicar
    private void OnInteractPressed()
    {
        if (askingNow) return;
        AdvanceDialogue();
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
            playerInput.InteractPressed -= OnInteractPressed;

        NarrativeDialogueData finished = currentDialogue;

        isDialogueOpen = false;
        currentDialogue = null;
        askForAcceptance = false;
        askingNow = false;

        if (declineButton != null)
            declineButton.style.display = DisplayStyle.None;

        dialogueRoot.style.display = DisplayStyle.None;

        DialogueEnded?.Invoke(finished);
    }

    // corta en fin de oracion si puede, si no en espacio, sin pasar de maxCharsPerPage
    private void SplitIntoPages(string text)
    {
        pages.Clear();
        text = string.IsNullOrEmpty(text) ? string.Empty : text.Trim();

        while (text.Length > maxCharsPerPage)
        {
            int cut = text.LastIndexOf(' ', maxCharsPerPage);
            int sentence = text.LastIndexOfAny(new[] { '.', '!', '?', '\u2026' }, maxCharsPerPage - 1);

            if (sentence > maxCharsPerPage * 0.5f) cut = sentence + 1;
            else if (cut <= 0) cut = maxCharsPerPage;

            pages.Add(text.Substring(0, cut).Trim());
            text = text.Substring(cut).Trim();
        }

        pages.Add(text);
    }
}