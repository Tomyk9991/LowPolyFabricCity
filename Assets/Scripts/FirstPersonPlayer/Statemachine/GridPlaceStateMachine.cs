using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FirstPersonPlayer.Statemachine
{
    [System.Serializable]
    public class RaycastOptions
    {
        public Transform PlayerTransform;
        public float RayLength;
        public LayerMask HitLayerMask;
    }

    public class GridPlaceStateMachine : Statemachine
    {
        private readonly float gridUnit;
        private readonly InputManager inputManager;
        private readonly GameObject[] inventoryGameObjects;
        
        private RaycastOptions raycastOptions;

        private GameObject currentPreviewGameObject;
        private int currentInventorySlot;

        private bool selectedSomethingWithoutPreview = false;

        public GridPlaceStateMachine(InputManager inputManager, List<GameObject> inventoryPrefabs, float gridUnit,
            RaycastOptions raycastOptions)
        {
            this.inputManager = inputManager;
            this.gridUnit = gridUnit;
            this.raycastOptions = raycastOptions;
            inventoryGameObjects = new GameObject[inventoryPrefabs.Count];
            
            for (int i = 0; i < inventoryPrefabs.Count; i++)
            {
                inventoryGameObjects[i] = Object.Instantiate(inventoryPrefabs[i]);
                inventoryGameObjects[i].SetActive(false);
            }

            if (inventoryGameObjects.Length > 0)
                currentPreviewGameObject = inventoryGameObjects[0];
            
            inputManager.PlayerInventorySlot.performed += OnInventorySlotSelected;
        }

        public override void OnDisable()
        {
            inputManager.PlayerInventorySlot.performed -= OnInventorySlotSelected;
        }

        private void OnInventorySlotSelected(InputAction.CallbackContext context)
        {
            currentPreviewGameObject.SetActive(false);
            
            currentInventorySlot = int.Parse(context.control.name);

            if (currentInventorySlot >= inventoryGameObjects.Length)
            {
                selectedSomethingWithoutPreview = true;
                return;
            }

            selectedSomethingWithoutPreview = false;
            
            currentPreviewGameObject = inventoryGameObjects[currentInventorySlot];
            currentPreviewGameObject.SetActive(true);
        }


        public override void OnUpdate()
        {
            if (selectedSomethingWithoutPreview) return;
            
            var ray = new Ray(raycastOptions.PlayerTransform.position, raycastOptions.PlayerTransform.forward);
            if (Physics.Raycast(ray, out var hit, raycastOptions.RayLength, raycastOptions.HitLayerMask))
            {
                if (currentPreviewGameObject.activeSelf == false)
                    currentPreviewGameObject.SetActive(true);
                
                currentPreviewGameObject.transform.position = hit.point;
            }
            else
            {
                if (currentPreviewGameObject.activeSelf)
                    currentPreviewGameObject.SetActive(false);
            }
        }
    }
}