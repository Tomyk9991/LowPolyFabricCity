using System.Collections.Generic;
using GridPlacement.PlaceTypes;
using UnityEngine;
using UnityEngine.Assertions;

namespace FirstPersonPlayer.Statemachine
{
    [System.Serializable]
    public class RaycastOptions
    {
        public Transform PlayerTransform;
        public float RayLength;
        public LayerMask HitLayerMask;
    }

    [System.Serializable]
    public class GridOptions
    {
        public float gridUnit;
        public float gridOffset;
    }
    
    public class GridPlaceStateMachine : Statemachine
    {
        private readonly InputManager inputManager;
        private readonly GridOptions gridOptions;
        private readonly GameObject[] inventoryGameObjects;

        private readonly RaycastOptions raycastOptions;

        private GameObject currentPreviewGameObject;
        private int currentInventorySlot;

        private bool selectedSomethingWithoutPreview;
        private Vector3Int? currentGridPosition;
        
        private readonly List<IPlaceType> placeTypes = new();
        private IPlaceType currentlyActivePlaceType = null;
        
        public GridPlaceStateMachine(InputManager inputManager, IReadOnlyList<GameObject> inventoryPrefabs, GridOptions gridOptions, Material previewMaterial, RaycastOptions raycastOptions)
        {
            this.inputManager = inputManager;
            this.gridOptions = gridOptions;
            this.raycastOptions = raycastOptions;
            
            var previewMaterialInstance = new Material(previewMaterial);
            inventoryGameObjects = new GameObject[inventoryPrefabs.Count];
            
            for (int i = 0; i < inventoryPrefabs.Count; i++)
            {
                var current = Object.Instantiate(inventoryPrefabs[i]);
                // change material to preview material
                Renderer[] renderers = current.GetComponentsInChildren<Renderer>();
                
                foreach (var renderer in renderers)
                {
                    Material[] materials = renderer.materials;
                
                    for (int j = 0; j < materials.Length; j++)
                        materials[j] = previewMaterialInstance;

                    renderer.materials = materials;
                }
                
                //disable gameObject by default
                current.SetActive(false);
                inventoryGameObjects[i] = current;
            }
            
            if (inventoryGameObjects.Length > 0)
            {
                currentPreviewGameObject = inventoryGameObjects[0];
                currentPreviewGameObject.SetActive(true);
                selectedSomethingWithoutPreview = false;
            }
            
            this.inputManager.PlayerInventorySlotSelected += OnInventorySlotSelected;
            this.inputManager.PlayerPlaceChanged += OnPlaceChanged;
        }

        private void OnPlaceChanged(bool clickActive)
        {
            if (!currentGridPosition.HasValue) return;
            
            // click begin
            if (clickActive)
            {
                currentlyActivePlaceType.OnClickTriggered(currentGridPosition.Value);
                return;
            }

            // click end
            currentlyActivePlaceType.OnClickReleased(currentGridPosition.Value);
        }
        
        
        private void OnInventorySlotSelected(int slot)
        {
            currentPreviewGameObject.SetActive(false);
            currentInventorySlot = slot;

            // invalid click. dont show preview
            if (currentInventorySlot >= inventoryGameObjects.Length)
            {
                selectedSomethingWithoutPreview = true;
                return;
            }

            // valid click. show preview
            selectedSomethingWithoutPreview = false;
            
            currentPreviewGameObject = inventoryGameObjects[currentInventorySlot];
            currentlyActivePlaceType = placeTypes[currentInventorySlot];
            currentPreviewGameObject.SetActive(true);
        }

        public override void OnDisable()
        {
            inputManager.PlayerInventorySlotSelected -= OnInventorySlotSelected;
            inputManager.PlayerPlaceChanged -= OnPlaceChanged;
        }
        
        public override void OnUpdate()
        {
            if (selectedSomethingWithoutPreview) return;

            var gridPosition = CastRay();
            currentlyActivePlaceType.OnUpdate(currentGridPosition);
            
            SetPreviewObjectTransform(gridPosition);
        }

        private void SetPreviewObjectTransform(Vector3? gridPosition)
        {
            if (gridPosition.HasValue)
            {
                if (!currentPreviewGameObject.activeSelf)
                    currentPreviewGameObject.SetActive(true);
                
                currentPreviewGameObject.transform.position = gridPosition.Value;
            }
            else
            {
                if (currentPreviewGameObject.activeSelf)
                    currentPreviewGameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Casts a ray from the player camera to the ground and returns the hit position
        /// </summary>
        /// <returns>Returns the hit position in grid space</returns>
        private Vector3? CastRay()
        {
            var ray = new Ray(raycastOptions.PlayerTransform.position, raycastOptions.PlayerTransform.forward);
            if (Physics.Raycast(ray, out var hit, raycastOptions.RayLength, raycastOptions.HitLayerMask))
            {
                var gridPosition = hit.point;
                currentGridPosition = new Vector3Int(Mathf.CeilToInt(gridPosition.x), 0, Mathf.CeilToInt(gridPosition.z));
                
                gridPosition.x = Mathf.CeilToInt(gridPosition.x) + gridOptions.gridOffset;
                gridPosition.y = 0.0f; //Mathf.CeilToInt(gridPosition.y);
                gridPosition.z = Mathf.CeilToInt(gridPosition.z) + gridOptions.gridOffset;

                return gridPosition;
            }

            this.currentGridPosition = null;
            return null;
        }

        public void AddPlaceType(IPlaceType newType)
        {
            placeTypes.Add(newType);
            currentlyActivePlaceType = placeTypes[currentInventorySlot];
        }
    }
}