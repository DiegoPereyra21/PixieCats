using UnityEngine;

// Hace que la burbuja mire siempre a la cámara y ajusta su tamaño según la distancia
[DefaultExecutionOrder(1000)] // después de Cinemachine, para que no se retrase
public class FairyPopupBillboard : MonoBehaviour
{
    [SerializeField] private Camera _camera; // si está vacía usa Camera.main

    [Header("Tamaño según distancia")]
    [SerializeField] private float _nearDistance = 3f;   // a esta distancia o menos: tamaño mínimo
    [SerializeField] private float _farDistance = 12f;   // a esta distancia o más: tamaño máximo
    [SerializeField] private float _nearScale = 0.6f;    // multiplicador del tamaño original cerca
    [SerializeField] private float _farScale = 1.6f;     // multiplicador lejos

    private Vector3 _baseScale;

    private void Start()
    {
        if (_camera == null) _camera = Camera.main;
        _baseScale = transform.localScale;
    }

    private void LateUpdate()
    {
        if (_camera == null) return;

        // misma rotación que la cámara: siempre de frente, sin inclinarse al mirar de arriba o de costado
        transform.rotation = _camera.transform.rotation;

        float distance = Vector3.Distance(_camera.transform.position, transform.position);
        float t = Mathf.InverseLerp(_nearDistance, _farDistance, distance);

        transform.localScale = _baseScale * Mathf.Lerp(_nearScale, _farScale, t);
    }
}