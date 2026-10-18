using UnityEngine;

// Hace que la burbuja mire siempre a la cámara
[DefaultExecutionOrder(1000)] // después de Cinemachine, para que no se retrase
public class FairyPopupBillboard : MonoBehaviour
{
    [SerializeField] private Camera _camera; // si está vacía usa Camera.main

    private void Start()
    {
        if (_camera == null) _camera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_camera == null) return;

        // misma rotación que la cámara: siempre de frente, sin inclinarse al mirar de arriba o de costado
        transform.rotation = _camera.transform.rotation;
    }
}