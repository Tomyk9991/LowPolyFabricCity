using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private FirstPersonPlayerControls playerControls;

    public Vector2 PlayerMovement => playerControls.Player.Movement.ReadValue<Vector2>();
    public Vector2 PlayerMouseDelta => playerControls.Player.Look.ReadValue<Vector2>();
    public InputAction PlayerInventorySlot => playerControls.Player.InventorySlot;
    public bool PlayerJumpedThisFrame => playerControls.Player.Jump.triggered;
    
    private void Awake()
    {
        playerControls = new FirstPersonPlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }
}
