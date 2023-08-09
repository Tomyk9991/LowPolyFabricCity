using System;
using System.Collections.Generic;
using FirstPersonPlayer.Statemachine;
using GridPlacement;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Ground Settings")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float rayLength = 0.2f;
    [SerializeField] private LayerMask groundedLayerMask;

    private GameObject hitObject;
    private Vector3 playerVelocity;

    [Header("Player movement Settings")]
    [SerializeField] private float playerSpeed = 2.0f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float gravityValue = -9.81f;

    [Header("Grid Settings")]
    [SerializeField] private float gridUnit = 1.0f;
    [SerializeField] private float gridOffset = 0.5f;
    [SerializeField] private RaycastOptions raycastOptions;
    [SerializeField] private List<GameObject> inventoryPrefabs;

    [Header("Preview Settings")]
    [SerializeField] private Material previewMaterial;
    [SerializeField] private AssemblyLineManager assemblyLineManager;
    
    private Vector3 halfPlayerHeight;

    private readonly PlayerStatemachineManager playerStatemachineManager = new();
    
    private void Start()
    {
        var movement = new MovementPlayerStatemachine(inputManager, cameraTransform, rayLength, groundedLayerMask, GetComponent<CharacterController>(), transform) 
        {
            PlayerSpeed = playerSpeed,
            JumpHeight = jumpHeight,
            GravityValue = gravityValue
        };
        
        playerStatemachineManager.AddState(movement);
        playerStatemachineManager.AddState(new CursorStateMachine());
        playerStatemachineManager.AddState(new GridPlaceStateMachine(inputManager, inventoryPrefabs, gridUnit, previewMaterial, assemblyLineManager, gridOffset, raycastOptions));

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        for (int i = playerStatemachineManager.Length - 1; i >= 0; i--)
        {
            var stateMachine = playerStatemachineManager[i];
            stateMachine.OnUpdate();
        }
    }
}
