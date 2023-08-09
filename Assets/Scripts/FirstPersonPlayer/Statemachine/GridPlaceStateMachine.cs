using System.Collections.Generic;
using GridPlacement;
using UnityEngine;

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
        private readonly float gridOffset;
        private readonly InputManager inputManager;
        private readonly GameObject[] inventoryGameObjects;

        private readonly Material previewMaterial;
        private readonly Material previewMaterialInstance;
        
        private readonly RaycastOptions raycastOptions;

        private GameObject currentPreviewGameObject;
        private int currentInventorySlot;

        private bool selectedSomethingWithoutPreview;
        private Vector3Int? currentGridPosition;

        private AssemblyLine line;
        private AssemblyLineManager assemblyLineManager;
        
        public GridPlaceStateMachine(InputManager inputManager, List<GameObject> inventoryPrefabs, float gridUnit, Material previewMaterial, AssemblyLineManager assemblyLineManager, float gridOffset,
            RaycastOptions raycastOptions)
        {
            this.inputManager = inputManager;
            this.gridUnit = gridUnit;
            this.assemblyLineManager = assemblyLineManager;
            this.gridOffset = gridOffset;
            this.raycastOptions = raycastOptions;
            previewMaterialInstance = new Material(previewMaterial);
            
            
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
                currentPreviewGameObject = inventoryGameObjects[0];
            
            inputManager.PlayerInventorySlotSelected += OnInventorySlotSelected;
            inputManager.PlayerPlaceChanged += OnPlaceChanged;
        }

        private void OnPlaceChanged(bool clickActive)
        {
            if (!currentGridPosition.HasValue) return;
            
            // click begin
            if (clickActive)
            {
                var (appendMode, assemblyLine) = assemblyLineManager.GetOrCreate(currentGridPosition.Value);
                this.line = assemblyLine;
                
                if (appendMode == AppendMode.InsertFront)
                    line.InsertFront(currentGridPosition.Value);
                else
                    line.AddNode(currentGridPosition.Value);
            }

            // click end
            if (!clickActive)
            {
                // check if hits something
                // if so, return bad
                line.AddNode(currentGridPosition.Value);
            }
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

            if (gridPosition.HasValue)
            {
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
                
                gridPosition.x = Mathf.CeilToInt(gridPosition.x) + gridOffset;
                gridPosition.y = 0.0f; //Mathf.CeilToInt(gridPosition.y);
                gridPosition.z = Mathf.CeilToInt(gridPosition.z) + gridOffset;

                return gridPosition;
            }

            this.currentGridPosition = null;
            return null;
        }
    }
}