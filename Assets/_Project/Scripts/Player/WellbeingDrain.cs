using UnityEngine;

public class WellbeingDrain : MonoBehaviour
{
    [SerializeField] private PlayerStats _stats;
    [SerializeField] private GameEndings _endings;

    private float _timer;

    private void Update()
    {
        if (_endings.HasEnded) return;
        if (NarrativeDialogueManager.Instance != null && NarrativeDialogueManager.Instance.IsDialogueOpen) return;

        _timer += Time.deltaTime;
        if (_timer < _stats.Rules.WellbeingDrainInterval) return;

        _timer -= _stats.Rules.WellbeingDrainInterval;
        _stats.Wellbeing.Add(-_stats.Rules.WellbeingDrainAmount);
    }
}