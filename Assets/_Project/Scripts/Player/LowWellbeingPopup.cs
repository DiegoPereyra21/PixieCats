using UnityEngine;

// Popup del hada cuando el bienestar baja del umbral; se rearma al recuperarse
public class LowWellbeingPopup : MonoBehaviour
{
    [SerializeField] private PlayerStats _stats;
    [SerializeField] private FairyPopupDialogueManager _popup;
    [SerializeField, Range(0f, 1f)] private float _threshold = 0.3f;
    [SerializeField, Min(0.1f)] private float _duration = 7f;
    [SerializeField, TextArea] private string _text = "Debo comer algún hongo morado antes de desmayarme del hambre";

    private bool _warned;
    private bool _pending;

    private void OnEnable()
    {
        _warned = _stats.Wellbeing.Normalized <= _threshold; // si ya arranca bajo, no avisa
        _stats.Wellbeing.Changed += OnChanged;
    }

    private void OnDisable() => _stats.Wellbeing.Changed -= OnChanged;

    private void OnChanged(float value, float delta)
    {
        bool low = _stats.Wellbeing.Normalized <= _threshold;

        if (low && !_warned) _pending = true;
        if (!low) _pending = false;

        _warned = low;
    }

    private void Update()
    {
        if (!_pending) return;
        if (NarrativeDialogueManager.Instance != null && NarrativeDialogueManager.Instance.IsDialogueOpen) return;

        _popup.ShowMessage(_text, _duration);
        _pending = false;
    }
}