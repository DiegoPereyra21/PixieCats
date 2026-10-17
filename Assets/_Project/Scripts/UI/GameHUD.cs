
using UnityEngine;
using UnityEngine.UIElements;

public class GameHUD : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private Label timeLabel;
    private GameProgressManager progressManager;

    private void Start()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        progressManager = GameProgressManager.Instance;

        if (uiDocument == null || progressManager == null)
        {
            Debug.LogError("GameHUD: falta UIDocument o GameProgressManager.");
            enabled = false;
            return;
        }

        timeLabel = uiDocument.rootVisualElement.Q<Label>("TimeLabel");

        if (timeLabel == null)
        {
            Debug.LogError("GameHUD: no se encontró el elemento time-label.");
            enabled = false;
            return;
        }

        UpdateTime();
    }

    private void Update()
    {
        if (progressManager == null || timeLabel == null)
            return;

        UpdateTime();
    }

    private void UpdateTime()
    {
        timeLabel.text = progressManager.CurrentTimeFormatted;
    }
}
