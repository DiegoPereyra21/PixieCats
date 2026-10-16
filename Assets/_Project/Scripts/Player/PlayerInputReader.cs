using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public bool Sprint { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpRequested { get; private set; }

    public event Action JumpPressed;

    private InputAction _move;
    private InputAction _sprint;
    private InputAction _jump;

    private void Awake()
    {
        _move = InputSystem.actions.FindAction("Player/Move");
        _sprint = InputSystem.actions.FindAction("Player/Sprint");
        _jump = InputSystem.actions.FindAction("Player/Jump");
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
    }

    public void ConsumeJump() => JumpRequested = false;
}