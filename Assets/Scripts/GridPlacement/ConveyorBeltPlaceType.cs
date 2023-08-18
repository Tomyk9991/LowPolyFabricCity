using System.Collections.Generic;
using System.Linq;
using Common.DataStructures;
using ExtensionMethods;
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

        private AppendMode? appendMode;
        
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
            var (mode, assemblyLine) = managers.AssemblyLineManager.GetOrCreate(currentGridPosition);
            line = assemblyLine;
            line.Finished = false;
            
            appendMode = mode;
            startGridPosition = currentGridPosition;
        }

        public void OnClickReleased(Vector3Int currentGridPosition)
        {
            if (startGridPosition == null) return;
            line.Finished = true;
            
            if (appendMode == AppendMode.Append)
                line.AddConnection(startGridPosition.Value, currentGridPosition, SolutionA);
            else
                line.InsertFrontConnection(startGridPosition.Value, currentGridPosition, SolutionA);

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
                ? AssemblyLine.GetPointsBetweenV1(startGridPosition.Value, currentGridPosition.Value)
                : AssemblyLine.GetPointsBetweenV2(startGridPosition.Value, currentGridPosition.Value);

            var positions = currentPreviewLine.Select(
                a => new Vector3(a.x + gridOptions.gridOffset, a.y + 0.01f, a.z + gridOptions.gridOffset)
            ).ToArray();
            
            previewOptions.LineRenderer.positionCount = currentPreviewLine.Count;
            previewOptions.LineRenderer.SetPositions(positions);

            CustomGradient gradient = new CustomGradient();
            
            for (int i = 0; i < positions.Length; i++)
            {
                var t = (float) i / positions.Length;
                gradient.AddKey(new GradientColorKey(i == 5 ? Color.red : Color.green, t));
            }
            
            
            previewOptions.LineRenderer.SetGradientFixed(gradient);
            previewOptions.LineRenderer.Simplify(.1f);
        }
    }
}