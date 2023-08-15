using System.Collections.Generic;
using System.Linq;
using FirstPersonPlayer;
using FirstPersonPlayer.Statemachine;
using GridPlacement.PlaceTypes;
using UnityEngine;

namespace GridPlacement
{
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
    
    public class ConveyorBeltPlaceType : IPlaceType
    {
        private readonly GridPlaceManagers managers;
        private readonly GridOptions gridOptions;
        private readonly PreviewOptions previewOptions;
        
        private AssemblyLine line;
        private bool SolutionA { get; set; } = true;
        private Vector3Int? startGridPosition;
        private List<Vector3Int> currentPreviewLine = new();
        
        public ConveyorBeltPlaceType(PlaceTypeGrepper placeTypeGrep)
        {
            previewOptions = new PreviewOptions
            {
                PreviewMaterial = placeTypeGrep.previewMaterial,
                LineRenderer = placeTypeGrep.previewPrefab.GetComponent<LineRenderer>()
            };
            
            managers = new GridPlaceManagers
            {
                InputManager = (InputManager) placeTypeGrep.managers[0],
                AssemblyLineManager = (AssemblyLineManager) placeTypeGrep.managers[1]
            };

            gridOptions = placeTypeGrep.gridOptions;
            
            managers.InputManager.PlayerPlacementSolutionChanged += (solution) => SolutionA = solution;
        }


        public void OnClickTriggered(Vector3Int currentGridPosition)
        {
            var (appendMode, assemblyLine) = managers.AssemblyLineManager.GetOrCreate(currentGridPosition);
            line = assemblyLine;
            line.Finished = false;
            
            if (appendMode == AppendMode.InsertFront)
                line.InsertFront(currentGridPosition, SolutionA);
            else
                line.AddNode(currentGridPosition, SolutionA);

            startGridPosition = currentGridPosition;
        }

        public void OnClickReleased(Vector3Int currentGridPosition)
        {
            line.Finished = true;
            line.AddNode(currentGridPosition, SolutionA);
            line = null;

            startGridPosition = null;
            previewOptions.LineRenderer.positionCount = 0;
            currentPreviewLine.Clear();
        }

        public void OnUpdate(Vector3Int? currentGridPosition)
        {
            CalculatePreviewLine(currentGridPosition);
        }
        
        private void CalculatePreviewLine(Vector3Int? currentGridPosition)
        {
            if (!currentGridPosition.HasValue || !startGridPosition.HasValue) return;
            
            currentPreviewLine = SolutionA
                ? AssemblyLineManager.GetPointsBetweenV1(startGridPosition.Value, currentGridPosition.Value)
                : AssemblyLineManager.GetPointsBetweenV2(startGridPosition.Value, currentGridPosition.Value);

            previewOptions.LineRenderer.positionCount = currentPreviewLine.Count;
            previewOptions.LineRenderer.SetPositions(currentPreviewLine.Select(
                    a => new Vector3(a.x + gridOptions.gridOffset, a.y + 0.01f, a.z + + gridOptions.gridOffset)
                ).ToArray()
            );
                
            previewOptions.LineRenderer.Simplify(.1f);
        }
    }
}