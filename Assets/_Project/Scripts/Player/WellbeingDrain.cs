using UnityEngine;

public class WellbeingDrain : MonoBehaviour
{
    [SerializeField] private PlayerStats _stats;
    [SerializeField] private GameEndings _endings;
    [SerializeField] private float _tickInterval = 1f; // por pasos, para no disparar Changed cada frame

    private float _timer;

    private void Update()
    {
        if (_endings.HasEnded) return;
        if (NarrativeDialogueManager.Instance != null && NarrativeDialogueManager.Instance.IsDialogueOpen) return;

        _timer += Time.deltaTime;
        if (_timer < _tickInterval) return;

        _timer -= _tickInterval;
        _stats.Wellbeing.Add(-_stats.Rules.WellbeingDrainPerSecond * _tickInterval);
    }
}