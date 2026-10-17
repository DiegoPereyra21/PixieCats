using System;
using UnityEngine;

public enum EndingType
{
    WellbeingDepleted,
    ReputationLost,
    ReputationMaxed,
    ChainFinished
}

public class GameEndings : MonoBehaviour
{
    [SerializeField] private PlayerStats _stats;
    [SerializeField] private MissionManager _manager;

    public bool HasEnded { get; private set; }

    public event Action<EndingType> EndingReached;

    private void OnEnable()
    {
        _stats.Wellbeing.Changed += OnWellbeingChanged;
        _stats.Reputation.Changed += OnReputationChanged;
        _manager.ChainFinished += OnChainFinished;
    }

    private void OnDisable()
    {
        _stats.Wellbeing.Changed -= OnWellbeingChanged;
        _stats.Reputation.Changed -= OnReputationChanged;
        _manager.ChainFinished -= OnChainFinished;
    }

    private void OnWellbeingChanged(float value, float delta)
    {
        if (value <= _stats.Wellbeing.Min) End(EndingType.WellbeingDepleted);
    }

    private void OnReputationChanged(float value, float delta)
    {
        if (value <= _stats.Reputation.Min) End(EndingType.ReputationLost);
        else if (value >= _stats.Reputation.Max) End(EndingType.ReputationMaxed);
    }

    private void OnChainFinished() => End(EndingType.ChainFinished);

    private void End(EndingType ending)
    {
        // gana el primer final que ocurra
        if (HasEnded) return;

        HasEnded = true;
        _manager.PauseTimer(); // que la misión en curso no falle después del final
        EndingReached?.Invoke(ending);
    }
}