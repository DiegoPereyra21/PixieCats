using UnityEngine;

[CreateAssetMenu(menuName = "Missions/Mission Rules")]
public class MissionRulesSO : ScriptableObject
{
    [Header("Bienestar")]
    [SerializeField] private float _startWellbeing = 100f;
    [SerializeField] private float _wellbeingCostPerMission = 20f;
    [SerializeField, Min(0)] private int _freeMissions = 3; // las primeras N aceptadas no cuestan

    [Header("Reputación")]
    [SerializeField] private float _startReputation = 50f;
    [SerializeField] private float _maxReputation = 100f;
    [SerializeField] private float _reputationOnComplete = 10f;
    [SerializeField] private float _reputationOnFail = 20f;

    public float StartWellbeing => _startWellbeing;
    public float WellbeingCostPerMission => _wellbeingCostPerMission;
    public int FreeMissions => _freeMissions;
    public float StartReputation => _startReputation;
    public float MaxReputation => _maxReputation;
    public float ReputationOnComplete => _reputationOnComplete;
    public float ReputationOnFail => _reputationOnFail;
}