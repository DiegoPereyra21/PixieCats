
using UnityEngine;
using UnityEngine.UIElements;

public class FairyPopupDialogueManager : MonoBehaviour
{
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

        if (displayTimer > 0f)
        {
            displayTimer -= Time.deltaTime;

            if (displayTimer <= 0f)
                popupPanel.style.display = DisplayStyle.None;
        }

        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            ShowRandomDialogue();
        }
    }

    private void ShowRandomDialogue()
    {
        if (dialogueTable == null || dialogueTable.Dialogues == null || dialogueTable.Dialogues.Length == 0)
            return;

        FairyPopupDialogueData dialogue = GetRandomDialogue();

        if (dialogue == null)
            return;

        lastDialogue = dialogue;
        popupText.text = dialogue.Text;

        popupPanel.style.display = DisplayStyle.Flex;
        displayTimer = displayDuration;
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
