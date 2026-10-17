using UnityEngine;

// Aplica las reglas de bienestar y reputación según lo que pasa con las misiones
public class MissionConsequences : MonoBehaviour
{
    [SerializeField] private MissionManager _manager;
    [SerializeField] private PlayerStats _stats;

    private int _acceptedCount;

    private void OnEnable()
    {
        _manager.MissionAccepted += OnAccepted;
        _manager.MissionCompleted += OnCompleted;
        _manager.AcceptDeadlineMissed += OnAcceptDeadlineMissed;
    }

    private void OnDisable()
    {
        _manager.MissionAccepted -= OnAccepted;
        _manager.MissionCompleted -= OnCompleted;
        _manager.AcceptDeadlineMissed -= OnAcceptDeadlineMissed;
    }

    private void OnAccepted(MissionDataSO mission)
    {
        _acceptedCount++;

        // las primeras misiones son gratis; desde ahí cada aceptación desgasta
        if (_acceptedCount > _stats.Rules.FreeMissions)
            _stats.Wellbeing.Add(-_stats.Rules.WellbeingCostPerMission);
    }

    private void OnCompleted(MissionDataSO mission, bool late)
    {
        _stats.Reputation.Add(late ? _stats.Rules.ReputationOnLateDelivery : _stats.Rules.ReputationOnComplete);
    }

    private void OnAcceptDeadlineMissed(MissionDataSO mission) => _stats.Reputation.Add(-_stats.Rules.ReputationOnLateAccept);
}