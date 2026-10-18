using UnityEngine;
using UnityEngine.UIElements;

// Panel de misión del HUD: alterna entre sin misión / pendiente de aceptar / aceptada y muestra textos, timers y la alerta
public class MissionPanelHUD : MonoBehaviour
{
    private const string HiddenClass = "is-hidden";

    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private MissionManager _manager;

    private VisualElement _emptyState;
    private VisualElement _pendingState;
    private VisualElement _acceptedState;
    private VisualElement _alert;
    private Label _pendingRequester;
    private Label _acceptedRequester;
    private Label _details;
    private Label _objective;
    private Label _searchTimer;
    private Label _completionTimer;
    private VisualElement _panel;
    private void Start()
    {
        if (_uiDocument == null) _uiDocument = GetComponent<UIDocument>();

        VisualElement root = _uiDocument.rootVisualElement;
        _emptyState = root.Q<VisualElement>("MissionEmptyState");
        _pendingState = root.Q<VisualElement>("MissionPendingState");
        _acceptedState = root.Q<VisualElement>("MissionAcceptedState");
        _alert = root.Q<VisualElement>("MissionAlert");
        _pendingRequester = root.Q<Label>("MissionRequesterName");
        _acceptedRequester = root.Q<Label>("MissionAcceptedRequester");
        _details = root.Q<Label>("MissionRequestDetails");
        _objective = root.Q<Label>("MissionObjective");
        _searchTimer = root.Q<Label>("MissionSearchTimer");
        _completionTimer = root.Q<Label>("MissionCompletionTimer");
        _panel = root.Q<VisualElement>("MissionPanel");

        if (_panel == null || _emptyState == null || _pendingState == null || _acceptedState == null || _alert == null
            || _pendingRequester == null || _acceptedRequester == null || _details == null || _objective == null
            || _searchTimer == null || _completionTimer == null)
        {
            Debug.LogError("MissionPanelHUD: faltan elementos del panel de misión en el UXML.", this);
            return;
        }

        _manager.StateChanged += Refresh;
        _manager.ProgressChanged += OnProgressChanged;
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
        _manager.ProgressChanged -= OnProgressChanged;
        _manager.AcceptTimerChanged -= OnAcceptTimerChanged;
        _manager.AcceptDeadlineMissed -= OnAcceptDeadlineMissed;
        _manager.TimerChanged -= OnTimerChanged;
    }

    private void Refresh()
    {
        MissionDataSO active = _manager.ActiveMission;
        MissionDataSO next = _manager.NextMission;
        bool accepted = active != null;
        bool pending = !accepted && next != null;

        SetVisible(_panel, accepted); // el recuadro entero solo existe con una misión activa
        SetVisible(_acceptedState, accepted);
        SetVisible(_pendingState, false);
        SetVisible(_emptyState, false);

        // "!" mientras hay una misión nueva por aceptar
        SetVisible(_alert, pending);


        if (accepted)
        {
            _acceptedRequester.text = RequesterName(active);
            _details.text = active.Title;
        }

        // sin límite de tiempo no hay timer que mostrar
        SetVisible(_completionTimer, accepted && active.TimeLimit > 0f);
    }

    private void OnProgressChanged(MissionDataSO mission, int held, int required) => _objective.text = $"Recolectar: {held}/{required}";

    private void OnAcceptTimerChanged(MissionDataSO next, float seconds)
    {
        SetVisible(_searchTimer, true);
        _searchTimer.text = FormatTime(seconds);
    }

    private void OnAcceptDeadlineMissed(MissionDataSO next) => SetVisible(_searchTimer, false);

    private void OnTimerChanged(MissionDataSO mission, float seconds) => _completionTimer.text = FormatTime(seconds);

    private static string RequesterName(MissionDataSO mission) => mission.Requester != null ? mission.Requester.CharacterName : mission.Title;

    private static void SetVisible(VisualElement element, bool visible) => element.EnableInClassList(HiddenClass, !visible);

    private static string FormatTime(float seconds)
    {
        int total = Mathf.CeilToInt(seconds);
        return $"{total / 60:00}:{total % 60:00}";
    }
}