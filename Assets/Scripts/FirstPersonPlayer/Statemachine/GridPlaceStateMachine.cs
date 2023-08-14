using System.Collections.Generic;
using System.Linq;
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

    [System.Serializable]
    public class GridOptions
    {
        public float GridUnit { get; set; }
        public float GridOffset { get; set; }
    }

    [System.Serializable]
    public class GridPlaceManagers
    {
        public InputManager InputManager { get; set; }
        public AssemblyLineManager AssemblyLineManager { get; set; }
    }
    
    [System.Serializable]
    public class PreviewOptions
    {
        public Material PreviewMaterial { get; set; }
        public LineRenderer LineRenderer { get; set; }
    }

    public class GridPlaceStateMachine : Statemachine
    {
        private GridPlaceManagers managers;
        private GridOptions gridOptions;
        private PreviewOptions previewOptions;
        
        private readonly GameObject[] inventoryGameObjects;

        public bool SolutionA { get; set; } = true;

        private readonly Material previewMaterialInstance;
        
        private readonly RaycastOptions raycastOptions;

        private GameObject currentPreviewGameObject;
        private int currentInventorySlot;

        private bool selectedSomethingWithoutPreview;
        private Vector3Int? currentGridPosition;

        private AssemblyLine line;

        private Vector3Int? startGridPosition;
        private Vector3Int? endGridPosition;
        private List<Vector3Int> currentPreviewLine = new();
        
        public GridPlaceStateMachine(GridPlaceManagers managers, List<GameObject> inventoryPrefabs, GridOptions gridOptions, PreviewOptions previewOptions, RaycastOptions raycastOptions)
        {
            this.previewOptions = previewOptions;
            this.managers = managers;
            this.gridOptions = gridOptions;
            this.raycastOptions = raycastOptions;
            
            previewMaterialInstance = new Material(previewOptions.PreviewMaterial);
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
            
            managers.InputManager.PlayerInventorySlotSelected += OnInventorySlotSelected;
            managers.InputManager.PlayerPlaceChanged += OnPlaceChanged;
            managers.InputManager.PlayerPlacementSolutionChanged += (solution) => SolutionA = solution;
        }

        private void OnPlaceChanged(bool clickActive)
        {
            if (!currentGridPosition.HasValue) return;
            
            // click begin
            if (clickActive)
            {
                var (appendMode, assemblyLine) = managers.AssemblyLineManager.GetOrCreate(currentGridPosition.Value);
                line = assemblyLine;
                
                if (appendMode == AppendMode.InsertFront)
                    line.InsertFront(currentGridPosition.Value, SolutionA);
                else
                    line.AddNode(currentGridPosition.Value, SolutionA);
                
                startGridPosition = currentGridPosition;
            }

            // click end
            if (!clickActive)
            {
                // check if hits something
                // if so, return bad
                line.AddNode(currentGridPosition.Value, SolutionA);
                line = null;
                
                startGridPosition = null;
                endGridPosition = null;
                previewOptions.LineRenderer.positionCount = 0;
                currentPreviewLine.Clear();
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
            managers.InputManager.PlayerInventorySlotSelected -= OnInventorySlotSelected;
            managers.InputManager.PlayerPlaceChanged -= OnPlaceChanged;
        }
        
        public override void OnUpdate()
        {
            if (selectedSomethingWithoutPreview) return;

            var gridPosition = CastRay();

            CalculatePreviewLine();
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

        private void CalculatePreviewLine()
        {
            if (!currentGridPosition.HasValue || !startGridPosition.HasValue) return;
            
            this.currentPreviewLine = SolutionA
                ? AssemblyLineManager.GetPointsBetweenV1(this.startGridPosition.Value,
                    this.currentGridPosition.Value)
                : AssemblyLineManager.GetPointsBetweenV2(this.startGridPosition.Value,
                    this.currentGridPosition.Value);

            this.previewOptions.LineRenderer.positionCount = currentPreviewLine.Count;
            this.previewOptions.LineRenderer.SetPositions(currentPreviewLine.Select(
                    a => new Vector3(a.x + gridOptions.GridOffset, a.y + 0.01f, a.z + + gridOptions.GridOffset)
                ).ToArray()
            );
                
            this.previewOptions.LineRenderer.Simplify(.1f);
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
                
                gridPosition.x = Mathf.CeilToInt(gridPosition.x) + gridOptions.GridOffset;
                gridPosition.y = 0.0f; //Mathf.CeilToInt(gridPosition.y);
                gridPosition.z = Mathf.CeilToInt(gridPosition.z) + gridOptions.GridOffset;

                return gridPosition;
            }

            this.currentGridPosition = null;
            return null;
        }
    }
}