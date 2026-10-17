using UnityEngine;

[CreateAssetMenu(menuName = "Missions/Mission Data")]
public class MissionDataSO : ScriptableObject
{
    [SerializeField] private string _title;
    [SerializeField] private PickupObject _itemPrefab;
    [SerializeField, Min(1)] private int _requiredCount = 1;
    [SerializeField, Min(0)] private int _extraCount = 2;    // ítems de más que aparecen
    [SerializeField, Min(0)] private float _timeLimit = 90f; // segundos; 0 = sin límite

    public string Title => _title;
    public PickupObject ItemPrefab => _itemPrefab;
    public int RequiredCount => _requiredCount;
    public int SpawnCount => _requiredCount + _extraCount;
    public float TimeLimit => _timeLimit;
}