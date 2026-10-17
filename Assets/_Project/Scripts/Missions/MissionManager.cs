using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    [SerializeField] private MissionChainSO _chain;
    [SerializeField] private Transform _spawnParent; // opcional, ordena la jerarquía

    public MissionDataSO ActiveMission => _active;

    public event Action StateChanged;                               // cualquier cambio de estado
    public event Action<MissionDataSO> MissionAccepted;
    public event Action<MissionDataSO, int, int> ProgressChanged;   // misión, juntados, requeridos
    public event Action<MissionDataSO> DeliveryRejected;            // se intentó entregar sin los ítems
    public event Action<MissionDataSO> MissionCompleted;
    public event Action AllMissionsCompleted;

    private readonly List<PickupObject> _spawned = new List<PickupObject>();
    private int _currentIndex;
    private MissionDataSO _active;
    private int _held;

    public MissionState GetState(MissionDataSO mission)
    {
        int index = _chain.IndexOf(mission);

        if (index < 0 || index > _currentIndex) return MissionState.Unavailable;
        if (index < _currentIndex) return MissionState.Completed;
        return _active == mission ? MissionState.InProgress : MissionState.Available;
    }

    // Acepta la misión y hace aparecer sus ítems en el área
    public bool TryAccept(MissionDataSO mission, SpawnArea area)
    {
        if (GetState(mission) != MissionState.Available) return false;

        _active = mission;
        _held = 0;
        SpawnItems(mission, area);

        MissionAccepted?.Invoke(mission);
        ProgressChanged?.Invoke(mission, _held, mission.RequiredCount);
        StateChanged?.Invoke();
        return true;
    }

    // Entrega los ítems de la bolsa; si alcanzan, completa la misión
    public bool TryDeliver(MissionDataSO mission, PlayerCarrier carrier)
    {
        if (GetState(mission) != MissionState.InProgress) return false;

        // solo cuentan los ítems de esta misión que ya están guardados en la bolsa
        Predicate<PickupObject> isMissionItem = _spawned.Contains;

        if (carrier.CountStored(isMissionItem) < mission.RequiredCount)
        {
            DeliveryRejected?.Invoke(mission);
            return false;
        }

        carrier.RemoveStored(isMissionItem, mission.RequiredCount);

        // se destruyen los entregados y los de más que quedaron en el mundo
        foreach (PickupObject item in _spawned)
        {
            if (item != null) Destroy(item.gameObject);
        }
        _spawned.Clear();

        _active = null;
        _held = 0;
        _currentIndex++;

        MissionCompleted?.Invoke(mission);
        StateChanged?.Invoke();

        if (_currentIndex >= _chain.Count) AllMissionsCompleted?.Invoke();
        return true;
    }

    private void SpawnItems(MissionDataSO mission, SpawnArea area)
    {
        List<Vector3> points = area.GetPoints(mission.SpawnCount);

        foreach (Vector3 point in points)
        {
            PickupObject item = Instantiate(mission.ItemPrefab, point, Quaternion.identity, _spawnParent);

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
        bool full = _held >= _active.RequiredCount;
        foreach (PickupObject item in _spawned)
        {
            if (!item.IsHeld) item.IsPickable = !full;
        }

        ProgressChanged?.Invoke(_active, _held, _active.RequiredCount);
    }
}