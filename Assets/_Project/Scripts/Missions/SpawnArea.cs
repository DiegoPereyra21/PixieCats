using System.Collections.Generic;
using UnityEngine;

public class SpawnArea : MonoBehaviour
{
    [SerializeField] private Vector3 _size = new Vector3(20f, 10f, 20f); // caja en espacio local
    [SerializeField] private LayerMask _groundMask = ~0;                 // solo la layer del piso
    [SerializeField] private float _minDistance = 2f;
    [SerializeField] private float _heightOffset = 0.05f;
    [SerializeField] private int _attemptsPerPoint = 30;

    // Devuelve hasta `count` posiciones sobre el piso, separadas entre sí
    public List<Vector3> GetPoints(int count)
    {
        List<Vector3> points = new List<Vector3>();

        for (int i = 0; i < count; i++)
        {
            if (TryFindPoint(points, out Vector3 point)) points.Add(point);
        }

        if (points.Count < count)
            Debug.LogWarning($"{name}: solo se encontraron {points.Count} de {count} posiciones. Revisá el tamaño del área y el Ground Mask.", this);

        return points;
    }

    private bool TryFindPoint(List<Vector3> existing, out Vector3 result)
    {
        result = default;
        bool hasFallback = false;

        for (int attempt = 0; attempt < _attemptsPerPoint; attempt++)
        {
            // punto al azar en la cara superior de la caja, con raycast hacia abajo hasta el piso
            Vector3 local = new Vector3(
                Random.Range(-_size.x, _size.x) * 0.5f,
                _size.y * 0.5f,
                Random.Range(-_size.z, _size.z) * 0.5f);
            Vector3 origin = transform.TransformPoint(local);
            float distance = _size.y * transform.lossyScale.y;

            if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, distance, _groundMask, QueryTriggerInteraction.Ignore))
                continue;

            // el último punto válido queda de respaldo por si no se logra respetar la distancia mínima
            result = hit.point + Vector3.up * _heightOffset;
            hasFallback = true;

            if (IsFarEnough(result, existing)) return true;
        }

        return hasFallback;
    }

    private bool IsFarEnough(Vector3 candidate, List<Vector3> existing)
    {
        foreach (Vector3 point in existing)
        {
            if (Vector3.Distance(candidate, point) < _minDistance) return false;
        }

        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(0.2f, 0.8f, 0.4f, 0.9f);
        Gizmos.DrawWireCube(Vector3.zero, _size);
        Gizmos.color = new Color(0.2f, 0.8f, 0.4f, 0.12f);
        Gizmos.DrawCube(Vector3.zero, _size);
    }
}