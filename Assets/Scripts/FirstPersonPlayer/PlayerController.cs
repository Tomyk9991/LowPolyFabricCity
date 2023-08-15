using System;
using System.Collections.Generic;
using Common.Managers;
using FirstPersonPlayer.Statemachine;
using GridPlacement;
using UnityEngine;
using UnityEngine.Serialization;


[System.Serializable]
public class PlaceTypeGrepper
{
    /// <summary>
    /// the managers, which is responsible for the needed data
    /// </summary>
    [FormerlySerializedAs("manager")] public List<Manager> managers;
    /// <summary>
    ///  all prefabs which are bound to the keys 0-9. For example using key 1 will place the second prefab in this list
    /// </summary>
    public List<GameObject> keyBoundPrefabs;

    public Material previewMaterial;
    public GameObject previewPrefab;

    public GridOptions gridOptions;
}

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
    [SerializeField] private List<PlaceTypeGrepper> placeTypes = null;
    
    
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

        var gridPlaceStateMachine = new GridPlaceStateMachine(inputManager, inventoryPrefabs, 
            new GridOptions
            {
                gridUnit = gridUnit,
                gridOffset = gridOffset
            }, 
            previewMaterial, raycastOptions
        );

        
        gridPlaceStateMachine.AddPlaceType(new ConveyorBeltPlaceType(placeTypes[0]));
        
        playerStatemachineManager.AddState(movement);
        playerStatemachineManager.AddState(new CursorStateMachine());
        playerStatemachineManager.AddState(gridPlaceStateMachine);

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

    private void OnDrawGizmos()
    {
        foreach (var stateMachine in playerStatemachineManager)
        {
            stateMachine.OnDrawGizmos();
        }
    }
}
