using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Missions/Mission Data")]
public class MissionDataSO : ScriptableObject
{
    [SerializeField] private string _title;
    [SerializeField] private PickupObject[] _itemPrefabs;                // variantes que se mezclan al aparecer
    [SerializeField, HideInInspector] private PickupObject _itemPrefab;  // campo anterior (un solo prefab), se pasa solo a la lista
    [SerializeField, Min(1)] private int _requiredCount = 1;
    [SerializeField, Min(0)] private int _extraCount = 2;          // ítems de más que aparecen
    [SerializeField, Min(0)] private float _timeLimit = 90f;       // segundos para entregar; 0 = sin límite
    [SerializeField, Min(0)] private float _acceptTimeLimit = 30f; // segundos para aceptarla tras completar la anterior; 0 = sin límite
    [SerializeField] private DialogueCharacterData _requester; // quien da la misión

    public DialogueCharacterData Requester => _requester;
    public string Title => _title;
    public PickupObject ItemPrefab => _itemPrefabs != null && _itemPrefabs.Length > 0 ? _itemPrefabs[0] : _itemPrefab;
    public int RequiredCount => _requiredCount;
    public int SpawnCount => _requiredCount + _extraCount;
    public float TimeLimit => _timeLimit;
    public float AcceptTimeLimit => _acceptTimeLimit;

    private void OnValidate()
    {
        // pasa el prefab único de antes a la lista, para no perder lo ya configurado
        if (_itemPrefab != null && (_itemPrefabs == null || _itemPrefabs.Length == 0))
        {
            _itemPrefabs = new[] { _itemPrefab };
            _itemPrefab = null;
        }
    }

    // Prefabs para N ítems, repartidos parejo entre las variantes y en orden mezclado
    public List<PickupObject> PickPrefabs(int count)
    {
        List<PickupObject> pool = new List<PickupObject>();
        if (_itemPrefabs != null)
        {
            foreach (PickupObject prefab in _itemPrefabs)
            {
                if (prefab != null) pool.Add(prefab);
            }
        }
        if (pool.Count == 0 && _itemPrefab != null) pool.Add(_itemPrefab);

        List<PickupObject> result = new List<PickupObject>(count);
        List<PickupObject> bag = new List<PickupObject>();

        while (result.Count < count && pool.Count > 0)
        {
            // se mezcla una "bolsa" con todas las variantes y se vacía antes de rellenarla
            if (bag.Count == 0)
            {
                bag.AddRange(pool);
                for (int i = bag.Count - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    (bag[i], bag[j]) = (bag[j], bag[i]);
                }
            }

            result.Add(bag[bag.Count - 1]);
            bag.RemoveAt(bag.Count - 1);
        }

        return result;
    }
}