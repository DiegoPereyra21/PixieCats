using System;
using UnityEngine;

[RequireComponent(typeof(InteractionHighlight))]
public class QuestGiver : MonoBehaviour, IInteractable, IGuideTarget
{
    [SerializeField] private MissionManager _manager;
    [SerializeField] private MissionDataSO _mission;
    [SerializeField] private SpawnArea _spawnArea;

    public MissionState State => _manager.GetState(_mission);
    public bool CanInteract => State == MissionState.Available || State == MissionState.InProgress;
    public bool IsBlocked => false;
    public bool WasDeliveredLate => _manager.WasLate(_mission); // para un diálogo distinto si se entregó tarde
    public bool IsGuideActive => CanInteract;
    public Vector3 GuidePosition => transform.position;

    // para los diálogos: cambia el estado de este NPC / se interactúa con él (con el estado previo a la acción)
    public event Action<MissionState> StateChanged;
    public event Action<MissionState> Interacted;

    private InteractionHighlight _highlight;
    private MissionState _lastState;

    private void Awake()
    {
        _highlight = GetComponent<InteractionHighlight>();
        if (_highlight == null) _highlight = gameObject.AddComponent<InteractionHighlight>();
    }

    private void OnEnable()
    {
        _lastState = State;
        _manager.StateChanged += OnManagerStateChanged;
    }

    private void OnDisable() => _manager.StateChanged -= OnManagerStateChanged;

    public void Interact(PlayerInteractor interactor)
    {
        MissionState state = State;
        Interacted?.Invoke(state);

        if (state == MissionState.Available) _manager.TryAccept(_mission, _spawnArea);
        else if (state == MissionState.InProgress) _manager.TryDeliver(_mission, interactor.Carrier);
    }

    public void SetHighlighted(bool highlighted) => _highlight.Set(highlighted);

    private void OnManagerStateChanged()
    {
        MissionState state = State;
        if (state == _lastState) return;

        _lastState = state;
        StateChanged?.Invoke(state);
    }
}