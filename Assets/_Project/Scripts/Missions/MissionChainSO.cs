using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Missions/Mission Chain")]
public class MissionChainSO : ScriptableObject
{
    // el orden de la lista es el orden en que se hacen las misiones
    [SerializeField] private List<MissionDataSO> _missions = new List<MissionDataSO>();

    public int Count => _missions.Count;
    public MissionDataSO this[int index] => _missions[index];

    public int IndexOf(MissionDataSO mission) => _missions.IndexOf(mission);
}