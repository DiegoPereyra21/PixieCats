using UnityEngine;
using UnityEngine.UIElements;

// Panel de misión del HUD: alterna entre sin misión / pendiente de aceptar / aceptada y muestra los timers
public class MissionPanelHUD : MonoBehaviour
{
    private const string HiddenClass = "is-hidden";

    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private MissionManager _manager;

    private VisualElement _emptyState;
    private VisualElement _pendingState;
    private VisualElement _acceptedState;
    private Label _searchTimer;
    private Label _completionTimer;

    private void Start()
    {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();

        VisualElement root = _uiDocument.rootVisualElement;
        _emptyState = root.Q<VisualElement>("MissionEmptyState");
        _pendingState = root.Q<VisualElement>("MissionPendingState");
        _acceptedState = root.Q<VisualElement>("MissionAcceptedState");
        _searchTimer = root.Q<Label>("MissionSearchTimer");
        _completionTimer = root.Q<Label>("MissionCompletionTimer");

        if (_emptyState == null || _pendingState == null || _acceptedState == null || _searchTimer == null || _completionTimer == null)
        {
            Debug.LogError("MissionPanelHUD: faltan elementos del panel de misión en el UXML.", this);
            return;
        }

        _manager.StateChanged += Refresh;
        _manager.AcceptTimerChanged += OnAcceptTimerChanged;
        _manager.AcceptDeadlineMissed += OnAcceptDeadlineMissed;
        _manager.TimerChanged += OnTimerChanged;

        SetVisible(_searchTimer, _manager.IsAcceptTimerRunning);
        Refresh();
    }

    private void OnDestroy()
    {
        if (_manager == null) return;

        _manager.StateChanged -= Refresh;
        _manager.AcceptTimerChanged -= OnAcceptTimerChanged;
        _manager.AcceptDeadlineMissed -= OnAcceptDeadlineMissed;
        _manager.TimerChanged -= OnTimerChanged;
    }

    private void Refresh()
    {
        bool accepted = _manager.ActiveMission != null;
        bool pending = !accepted && _manager.NextMission != null;

        SetVisible(_acceptedState, accepted);
        SetVisible(_pendingState, pending);
        SetVisible(_emptyState, !accepted && !pending);

        // sin límite de tiempo no hay timer que mostrar
        SetVisible(_completionTimer, accepted && _manager.ActiveMission.TimeLimit > 0f);
    }

    private void OnAcceptTimerChanged(MissionDataSO next, float seconds)
    {
        SetVisible(_searchTimer, true);
        _searchTimer.text = FormatTime(seconds);
    }

    private void OnAcceptDeadlineMissed(MissionDataSO next) => SetVisible(_searchTimer, false);

    private void OnTimerChanged(MissionDataSO mission, float seconds) => _completionTimer.text = FormatTime(seconds);

    private static void SetVisible(VisualElement element, bool visible) => element.EnableInClassList(HiddenClass, !visible);

    private static string FormatTime(float seconds)
    {
        int total = Mathf.CeilToInt(seconds);
        return $"{total / 60:00}:{total % 60:00}";
    }
}