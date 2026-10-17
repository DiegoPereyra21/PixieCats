using UnityEngine;

// Muestra en consola misiones, medidores y finales hasta que exista la UI
public class GameDebugLogger : MonoBehaviour
{
    [SerializeField] private MissionManager _manager;
    [SerializeField] private PlayerStats _stats;
    [SerializeField] private GameEndings _endings;
    [SerializeField] private float _timerLogInterval = 10f;

    private int _lastTimerStep = -1;

    private void OnEnable()
    {
        _manager.MissionAccepted += OnAccepted;
        _manager.ProgressChanged += OnProgress;
        _manager.DeliveryRejected += OnRejected;
        _manager.MissionCompleted += OnCompleted;
        _manager.MissionFailed += OnFailed;
        _manager.TimerChanged += OnTimer;
        _stats.Wellbeing.Changed += OnWellbeing;
        _stats.Reputation.Changed += OnReputation;
        _endings.EndingReached += OnEnding;
    }

    private void OnDisable()
    {
        _manager.MissionAccepted -= OnAccepted;
        _manager.ProgressChanged -= OnProgress;
        _manager.DeliveryRejected -= OnRejected;
        _manager.MissionCompleted -= OnCompleted;
        _manager.MissionFailed -= OnFailed;
        _manager.TimerChanged -= OnTimer;
        _stats.Wellbeing.Changed -= OnWellbeing;
        _stats.Reputation.Changed -= OnReputation;
        _endings.EndingReached -= OnEnding;
    }

    private void Start()
    {
        Debug.Log($"[Stats] Bienestar {_stats.Wellbeing.Value:0} | Reputación {_stats.Reputation.Value:0}");
    }

    private void OnAccepted(MissionDataSO m)
    {
        _lastTimerStep = -1;
        string time = m.TimeLimit > 0f ? $"{m.TimeLimit:0} s" : "sin límite";
        Debug.Log($"[Misión] Aceptada: {m.Title} ({m.RequiredCount} ítems, {time})");
    }

    private void OnProgress(MissionDataSO m, int held, int required) => Debug.Log($"[Misión] {m.Title}: {held}/{required}");

    private void OnRejected(MissionDataSO m) => Debug.Log($"[Misión] {m.Title}: te faltan ítems para entregar");

    private void OnCompleted(MissionDataSO m) => Debug.Log($"[Misión] Completada: {m.Title}");

    private void OnFailed(MissionDataSO m) => Debug.Log($"[Misión] Fallida por tiempo: {m.Title}");

    private void OnTimer(MissionDataSO m, float remaining)
    {
        // solo loguea cada `_timerLogInterval` segundos
        int step = Mathf.CeilToInt(remaining / _timerLogInterval);
        if (step == _lastTimerStep) return;

        _lastTimerStep = step;
        Debug.Log($"[Misión] {m.Title}: quedan {remaining:0} s");
    }

    private void OnWellbeing(float value, float delta) => Debug.Log($"[Stats] Bienestar {value:0} ({delta:+0;-0})");

    private void OnReputation(float value, float delta) => Debug.Log($"[Stats] Reputación {value:0} ({delta:+0;-0})");

    private void OnEnding(EndingType ending) => Debug.Log($"[Final] {ending}");
}