using UnityEngine;

public class WellbeingDrain : MonoBehaviour
{
    [SerializeField] private PlayerStats _stats;

    private float _timer;

    private void Update()
    {
        if (_stats == null)
        {
            Debug.LogWarning("[WellbeingDrain] Falta asignar PlayerStats.");
            return;
        }

        if (GameProgressManager.Instance != null && GameProgressManager.Instance.HasEndingTriggered)
        {
            Debug.Log("[WellbeingDrain] Detenido: ya se disparó un final.");
            return;
        }

        if (NarrativeDialogueManager.Instance != null && NarrativeDialogueManager.Instance.IsDialogueOpen)
        {
            Debug.Log("[WellbeingDrain] Detenido: hay un diálogo abierto.");
            return;
        }

        _timer += Time.deltaTime;
        if (_timer < _stats.Rules.WellbeingDrainInterval) return;

        _timer -= _stats.Rules.WellbeingDrainInterval;
        _stats.Wellbeing.Add(-_stats.Rules.WellbeingDrainAmount);
    }
}