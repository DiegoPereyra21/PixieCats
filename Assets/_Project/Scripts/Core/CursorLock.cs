using UnityEngine;
using UnityEngine.InputSystem;

public class CursorLock : MonoBehaviour
{
    private void Start()
    {
        // en WebGL el navegador solo deja bloquear el cursor tras un click del jugador
#if !UNITY_WEBGL || UNITY_EDITOR
        Lock();
#endif
    }

    private void Update()
    {
        // en WebGL se bloquea con el primer click, y se vuelve a bloquear tras Esc
        if (Cursor.lockState != CursorLockMode.Locked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            Lock();
    }

    private void Lock()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}