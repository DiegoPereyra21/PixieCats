using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorLock : MonoBehaviour
{
    [SerializeField] private CinemachineInputAxisController _cameraInput; // si queda vacío se busca solo

    private bool _wasInDialogue;

    private void Start()
    {
        if (_cameraInput == null)
            _cameraInput = FindFirstObjectByType<CinemachineInputAxisController>();

        // en WebGL el navegador solo deja bloquear el cursor tras un click del jugador
#if !UNITY_WEBGL || UNITY_EDITOR
        Lock();
#endif
    }

    private void Update()
    {
        bool inDialogue = NarrativeDialogueManager.Instance != null && NarrativeDialogueManager.Instance.IsDialogueOpen;

        if (inDialogue)
        {
            if (!_wasInDialogue)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (_cameraInput != null)
                    _cameraInput.enabled = false; // el mouse no gira la cámara mientras se usa de puntero
            }

            _wasInDialogue = true;
            return;
        }

        if (_wasInDialogue)
        {
            _wasInDialogue = false;

            if (_cameraInput != null)
                _cameraInput.enabled = true;

            Lock();
            return;
        }

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