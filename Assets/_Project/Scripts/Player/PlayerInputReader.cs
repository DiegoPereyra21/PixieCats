using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    private const float BrakeThreshold = -0.5f; // también cubre diagonales con S

    public Vector2 Move { get; private set; }
    public bool Sprint { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool InteractHeld { get; private set; }
    public bool BrakeHeld => Move.y < BrakeThreshold;
    public float LastJumpPressTime { get; private set; } = float.NegativeInfinity;

    public event Action JumpPressed;
    public event Action InteractPressed;

    private InputAction _move;
    private InputAction _sprint;
    private InputAction _jump;
    private InputAction _interact;

    private void Awake()
    {
        _move = InputSystem.actions.FindAction("Player/Move");
        _sprint = InputSystem.actions.FindAction("Player/Sprint");
        _jump = InputSystem.actions.FindAction("Player/Jump");
        _interact = InputSystem.actions.FindAction("Player/Interact");
    }

    private void Update()
    {
        // con un diálogo abierto el jugador no se mueve ni salta
        bool blocked = NarrativeDialogueManager.Instance != null && NarrativeDialogueManager.Instance.IsDialogueOpen;

        Move = blocked ? Vector2.zero : _move.ReadValue<Vector2>();
        Sprint = !blocked && _sprint.IsPressed();
        JumpHeld = !blocked && _jump.IsPressed();
        InteractHeld = !blocked && _interact.IsPressed();

        if (_jump.WasPressedThisFrame() && !blocked)
        {
            // se guarda la hora antes del evento: si el vuelo usa este Space, lo consume
            LastJumpPressTime = Time.time;
            JumpPressed?.Invoke();
        }

        if (_interact.WasPressedThisFrame()) InteractPressed?.Invoke();
    }

    // true si hubo un Space dentro de la ventana de buffer
    public bool JumpBuffered(float window) => Time.time - LastJumpPressTime <= window;

    public void ConsumeJump() => LastJumpPressTime = float.NegativeInfinity;
}