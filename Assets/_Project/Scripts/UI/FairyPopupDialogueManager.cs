using UnityEngine;
using UnityEngine.UIElements;

public class FairyPopupDialogueManager : MonoBehaviour
{
    private const string VisibleClass = "fairy-popup-panel--visible";
    private const long HideDelayMs = 300; // un poco más que la transición del USS (0.25 s)

    [Header("Referencias")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private FairyPopupDialogueTable dialogueTable;

    [Header("Configuracion")]
    [SerializeField, Min(0.1f)] private float interval = 5f;
    [SerializeField, Min(0.1f)] private float displayDuration = 4f;

    private VisualElement popupPanel;
    private Label popupText;

    private float timer;
    private float displayTimer;
    private FairyPopupDialogueData lastDialogue;
    private IVisualElementScheduledItem hideItem;
    private bool forcedMessage; // popup lanzado por código: se muestra aunque haya un diálogo abierto

    private void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("FairyPopupDialogueManager: falta UIDocument.");
            enabled = false;
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;

        popupPanel = root.Q<VisualElement>("PopupPanel");
        popupText = root.Q<Label>("PopupText");

        if (popupPanel == null || popupText == null)
        {
            Debug.LogError("FairyPopupDialogueManager: no se encontraron PopupPanel o PopupText.");

            enabled = false;
            return;
        }

        popupPanel.style.display = DisplayStyle.None;
    }

    private void Update()
    {
        if (popupPanel == null)
            return;

        // durante un diálogo no hay popups aleatorios: se cierra el activo y se reinicia el intervalo
        if (!forcedMessage && NarrativeDialogueManager.Instance != null && NarrativeDialogueManager.Instance.IsDialogueOpen)
        {
            timer = 0f;

            if (displayTimer > 0f)
            {
                displayTimer = 0f;
                HidePopup();
            }

            return;
        }

        if (!forcedMessage)
        {
            timer += Time.deltaTime;

            if (timer >= interval)
            {
                timer = 0f;
                ShowRandomDialogue();
            }
        }

        if (displayTimer > 0f)
        {
            displayTimer -= Time.deltaTime;

            if (displayTimer <= 0f)
            {
                HidePopup();
                forcedMessage = false;
                timer = 0f;
            }
        }
    }

    public void ShowMessage(string text)
    {
        if (popupPanel == null || string.IsNullOrEmpty(text))
            return;

        forcedMessage = true;
        popupText.text = text;

        ShowPopup();
        displayTimer = displayDuration;
        timer = 0f;
    }

    private void ShowRandomDialogue()
    {
        if (dialogueTable == null)
        {
            Debug.LogError("FairyPopup: falta asignar Dialogue Table.");
            return;
        }

        if (dialogueTable.Dialogues == null ||
            dialogueTable.Dialogues.Length == 0)
        {
            Debug.LogWarning("FairyPopup: la tabla no contiene diálogos.");
            return;
        }

        FairyPopupDialogueData dialogue = GetRandomDialogue();

        if (dialogue == null)
        {
            Debug.LogWarning("FairyPopup: no se pudo elegir un diálogo.");
            return;
        }

        lastDialogue = dialogue;
        popupText.text = dialogue.Text;

        Debug.Log($"FairyPopup: mostrando diálogo: {dialogue.Text}");

        ShowPopup();
        displayTimer = displayDuration;
    }

    private void ShowPopup()
    {
        hideItem?.Pause(); // si se estaba cerrando, se cancela

        popupPanel.style.display = DisplayStyle.Flex;

        // la clase se agrega un instante después para que la transición del USS se vea
        popupPanel.schedule.Execute(() => popupPanel.AddToClassList(VisibleClass)).StartingIn(30);
    }

    private void HidePopup()
    {
        popupPanel.RemoveFromClassList(VisibleClass);

        // display none recién cuando terminó de achicarse
        hideItem = popupPanel.schedule.Execute(() => popupPanel.style.display = DisplayStyle.None).StartingIn(HideDelayMs);
    }

    private FairyPopupDialogueData GetRandomDialogue()
    {
        FairyPopupDialogueData[] dialogues = dialogueTable.Dialogues;

        if (dialogues.Length == 1)
            return dialogues[0];

        FairyPopupDialogueData dialogue = null;

        for (int i = 0; i < 10; i++)
        {
            dialogue = dialogues[Random.Range(0, dialogues.Length)];

            if (dialogue != lastDialogue)
                return dialogue;
        }

        return dialogue;
    }
}