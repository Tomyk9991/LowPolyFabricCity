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


    public event Action<int> PlayerInventorySlotSelected;
    public event Action<bool> PlayerPlaceChanged;
    
    
    private void Awake()
    {
        playerControls = new FirstPersonPlayerControls();
        playerControls.Player.InventorySlot.performed += ctx => PlayerInventorySlotSelected?.Invoke(int.Parse(ctx.control.name));
        
        playerControls.Player.Place.performed += _ => PlayerPlaceChanged?.Invoke(true);
        playerControls.Player.Place.canceled += _ => PlayerPlaceChanged?.Invoke(false);
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
