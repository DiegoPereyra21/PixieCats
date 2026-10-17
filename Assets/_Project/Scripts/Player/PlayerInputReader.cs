using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    private const float BrakeThreshold = -0.5f; // también cubre diagonales con S

    public Vector2 Move { get; private set; }
    public bool Sprint { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpRequested { get; private set; }
    public bool BrakeHeld => Move.y < BrakeThreshold;

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
        Move = _move.ReadValue<Vector2>();
        Sprint = _sprint.IsPressed();
        JumpHeld = _jump.IsPressed();

        if (_jump.WasPressedThisFrame())
        {
            // se guarda hasta el próximo FixedUpdate para no perder el salto
            JumpRequested = true;
            JumpPressed?.Invoke();
        }

        if (_interact.WasPressedThisFrame()) InteractPressed?.Invoke();
    }

    public void ConsumeJump() => JumpRequested = false;
}