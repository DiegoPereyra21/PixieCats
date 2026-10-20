using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    [SerializeField] private MissionChainSO _chain;
    [SerializeField] private Transform _spawnParent; // opcional, ordena la jerarquía

    public MissionDataSO ActiveMission => _active;
    public float TimeRemaining => _timeRemaining;
    public bool IsActiveMissionLate => _active != null && _late.Contains(_active);
    public bool IsAcceptTimerRunning => _acceptTimerRunning;
    public float AcceptTimeRemaining => _acceptTimeRemaining;
    public bool IsChainComplete => _currentIndex >= _chain.Count;
    // próxima misión por aceptar (null si hay una activa o ya no quedan)
    public MissionDataSO NextMission => _active == null && _currentIndex < _chain.Count ? _chain[_currentIndex] : null;
    public bool IsTimerPaused => _pauseRequests > 0;

    public event Action StateChanged;                                // cualquier cambio de estado
    public event Action<MissionDataSO> MissionAccepted;
    public event Action<MissionDataSO, int, int> ProgressChanged;    // misión, juntados, requeridos
    public event Action<MissionDataSO> DeliveryRejected;             // se intentó entregar sin los ítems
    public event Action<MissionDataSO, bool> MissionCompleted;       // misión, entregada tarde
    public event Action<MissionDataSO> MissionLate;                  // se terminó el tiempo de entrega
    public event Action<MissionDataSO, float> TimerChanged;          // misión, segundos para entregar
    public event Action<MissionDataSO, float> AcceptTimerChanged;    // próxima misión, segundos para aceptar
    public event Action<MissionDataSO> AcceptDeadlineMissed;         // no se aceptó a tiempo
    public event Action ChainFinished;                               // no quedan misiones
    private int _delivered; // ítems ya entregados de la misión activa
    private readonly List<PickupObject> _spawned = new List<PickupObject>();
    private readonly HashSet<MissionDataSO> _late = new HashSet<MissionDataSO>();
    private int _currentIndex;
    private MissionDataSO _active;
    private int _held;
    private float _timeRemaining;
    private bool _acceptTimerRunning;
    private float _acceptTimeRemaining;
    private int _pauseRequests;

    private void Update()
    {
        if (IsTimerPaused) return;

        // entre misiones corre el de aceptar; durante una misión, el de entrega
        if (_acceptTimerRunning) TickAcceptTimer();
        else if (_active != null && _active.TimeLimit > 0f && !_late.Contains(_active)) TickDeliveryTimer();
    }

    public MissionState GetState(MissionDataSO mission)
    {
        int index = _chain.IndexOf(mission);

        if (index < 0 || index > _currentIndex) return MissionState.Unavailable;
        if (index < _currentIndex) return MissionState.Completed;
        return _active == mission ? MissionState.InProgress : MissionState.Available;
    }

    // true si la misión se entregó (o se está haciendo) fuera de tiempo
    public bool WasLate(MissionDataSO mission) => _late.Contains(mission);

    // Pausa ambos temporizadores (diálogos, finales); cada PauseTimer necesita su ResumeTimer
    public void PauseTimer() => _pauseRequests++;
    public void ResumeTimer() => _pauseRequests = Mathf.Max(0, _pauseRequests - 1);

    // Acepta la misión, hace aparecer sus ítems en el área y arranca el tiempo de entrega
    public bool TryAccept(MissionDataSO mission, SpawnArea area)
    {
        if (GetState(mission) != MissionState.Available) return false;

        _acceptTimerRunning = false;
        _active = mission;
        _held = 0;
        _delivered = 0;
        _timeRemaining = mission.TimeLimit;
        SpawnItems(mission, area);

        MissionAccepted?.Invoke(mission);
        ProgressChanged?.Invoke(mission, _held, mission.RequiredCount);
        if (mission.TimeLimit > 0f) TimerChanged?.Invoke(mission, _timeRemaining);
        StateChanged?.Invoke();
        return true;
    }

    // Entrega los ítems de la bolsa; si alcanzan, completa la misión (a tiempo o tarde)
    // Entrega los ítems de la bolsa que sirvan para la misión; se puede hacer por partes y se completa al llegar al total
    public bool TryDeliver(MissionDataSO mission, PlayerCarrier carrier)
    {
        if (GetState(mission) != MissionState.InProgress) return false;

        // solo cuentan los ítems de esta misión que ya están guardados en la bolsa
        Predicate<PickupObject> isMissionItem = _spawned.Contains;

        int missing = mission.RequiredCount - _delivered;
        int available = carrier.CountStored(isMissionItem);

        if (available <= 0)
        {
            DeliveryRejected?.Invoke(mission);
            return false;
        }

        // se entregan solo los que faltan, aunque lleve de más
        List<PickupObject> removed = carrier.RemoveStored(isMissionItem, Mathf.Min(available, missing));

        foreach (PickupObject item in removed)
        {
            _spawned.Remove(item);
            Destroy(item.gameObject);
        }

        _delivered += removed.Count;
        _held -= removed.Count; // lo entregado ya no está en la bolsa

        // entrega parcial: la misión sigue activa
        if (_delivered < mission.RequiredCount)
        {
            ProgressChanged?.Invoke(mission, _held + _delivered, mission.RequiredCount);
            return true;
        }

        bool late = _late.Contains(mission);

        ClearSpawned(); // se destruyen los de más que quedaron en el mundo

        _active = null;
        _held = 0;
        _delivered = 0;
        _timeRemaining = 0f;
        _currentIndex++;

        MissionCompleted?.Invoke(mission, late);
        StateChanged?.Invoke();

        StartAcceptTimer();
        if (_currentIndex >= _chain.Count) ChainFinished?.Invoke();
        return true;
    }

    private void TickDeliveryTimer()
    {
        _timeRemaining = Mathf.Max(0f, _timeRemaining - Time.deltaTime);
        TimerChanged?.Invoke(_active, _timeRemaining);

        // al vencer no se pierde nada: la misión sigue y se entrega tarde
        if (_timeRemaining <= 0f)
        {
            _late.Add(_active);
            MissionLate?.Invoke(_active);
        }
    }

    private void StartAcceptTimer()
    {
        // después de la última misión no hay nada que aceptar
        if (_currentIndex >= _chain.Count) return;

        MissionDataSO next = _chain[_currentIndex];
        if (next.AcceptTimeLimit <= 0f) return;

        _acceptTimerRunning = true;
        _acceptTimeRemaining = next.AcceptTimeLimit;
        AcceptTimerChanged?.Invoke(next, _acceptTimeRemaining);
    }

    private void TickAcceptTimer()
    {
        MissionDataSO next = _chain[_currentIndex];

        _acceptTimeRemaining = Mathf.Max(0f, _acceptTimeRemaining - Time.deltaTime);
        AcceptTimerChanged?.Invoke(next, _acceptTimeRemaining);

        // penaliza una sola vez; la misión se puede aceptar igual
        if (_acceptTimeRemaining <= 0f)
        {
            _acceptTimerRunning = false;
            AcceptDeadlineMissed?.Invoke(next);
        }
    }

    private void ClearSpawned()
    {
        foreach (PickupObject item in _spawned)
        {
            if (item != null) Destroy(item.gameObject);
        }
        _spawned.Clear();
    }

    private void SpawnItems(MissionDataSO mission, SpawnArea area)
    {
        List<Vector3> points = area.GetPoints(mission.SpawnCount);
        List<PickupObject> prefabs = mission.PickPrefabs(points.Count);

        if (prefabs.Count == 0)
        {
            Debug.LogError($"MissionManager: la misión '{mission.Title}' no tiene prefabs de ítem asignados.", this);
            return;
        }

        for (int i = 0; i < points.Count; i++)
        {
            PickupObject item = Instantiate(prefabs[i], points[i], Quaternion.identity, _spawnParent);

            // se apoya en el piso según su collider (se asume el pivot en el centro)
            Collider col = item.GetComponentInChildren<Collider>();
            if (col != null) item.transform.position += Vector3.up * col.bounds.extents.y;

            item.Grabbed += OnItemGrabbed;
            item.Dropped += OnItemDropped;
            _spawned.Add(item);
        }
    }

    private void OnItemGrabbed(PickupObject item)
    {
        _held++;
        RefreshProgress();
    }

    private void OnItemDropped(PickupObject item)
    {
        _held--;
        RefreshProgress();
    }

    private void RefreshProgress()
    {
        if (_active == null) return;

        // al llegar al cupo no se pueden agarrar más ítems de la misión
        bool full = _held + _delivered >= _active.RequiredCount;
        foreach (PickupObject item in _spawned)
        {
            if (!item.IsHeld) item.IsPickable = !full;
        }

        ProgressChanged?.Invoke(_active, _held + _delivered, _active.RequiredCount);
    }
}