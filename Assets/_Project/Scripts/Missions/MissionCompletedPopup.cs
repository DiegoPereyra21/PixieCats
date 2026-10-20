using UnityEngine;

// Popup del hada al completar una misión; espera a que cierre el diálogo de entrega
public class MissionCompletedPopup : MonoBehaviour
{
    [SerializeField] private MissionManager _manager;
    [SerializeField] private FairyPopupDialogueManager _popup;
    [SerializeField, Min(0.1f)] private float _duration = 7f;
    [SerializeField, TextArea] private string _nextMissionText = "Debo ir rápidamente a ayudar al próximo humano";
    [SerializeField, TextArea] private string _lastMissionText = "Debo apurarme para volver a casa, sino quedaré afuera";

    private string _pending;

    private void OnEnable() => _manager.MissionCompleted += OnCompleted;

    private void OnDisable() => _manager.MissionCompleted -= OnCompleted;

    private void OnCompleted(MissionDataSO mission, bool late)
    {
        _pending = _manager.IsChainComplete ? _lastMissionText : _nextMissionText;
    }

    private void Update()
    {
        if (_pending == null) return;
        if (NarrativeDialogueManager.Instance != null && NarrativeDialogueManager.Instance.IsDialogueOpen) return;

        _popup.ShowMessage(_pending, _duration);
        _pending = null;
    }
}