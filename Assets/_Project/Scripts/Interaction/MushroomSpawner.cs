using UnityEngine;

// Gameplay se recarga cada noche, así que los hongos comidos vuelven a aparecer al otro día
public class MushroomSpawner : MonoBehaviour
{
    [System.Serializable]
    private class Zone
    {
        public SpawnArea Area;
        [Min(0)] public int Count = 3;
    }

    [SerializeField] private EdibleMushroom[] _prefabs;
    [SerializeField] private Zone[] _zones;
    [SerializeField] private Transform _spawnParent; // opcional, ordena la jerarquía

    private void Start()
    {
        if (_prefabs == null || _prefabs.Length == 0)
        {
            Debug.LogError("MushroomSpawner: no hay prefabs de hongo asignados.", this);
            return;
        }

        foreach (Zone zone in _zones)
        {
            if (zone.Area == null) continue;

            // se asume el pivot del hongo en la base
            foreach (Vector3 point in zone.Area.GetPoints(zone.Count))
            {
                EdibleMushroom prefab = _prefabs[Random.Range(0, _prefabs.Length)];
                Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                Instantiate(prefab, point, rotation, _spawnParent);
            }
        }
    }
}