using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GridPlacement
{
    public class AssemblyLineManager : MonoBehaviour
    {
        private readonly List<AssemblyLine> assemblyLines = new();

        [Header("Assembly line settings")]
        [SerializeField] private GameObject assemblyLinePrefab;
        [SerializeField] private GameObject assemblyLineCornerPrefab;
        

        [SerializeField] private float rotationOffset = 90.0f;
        [SerializeField] private Vector3 placementOffset = new(-0.5f, 0, -0.5f);


        public (AppendMode, AssemblyLine) GetOrCreate(Vector3Int currentGridPosition)
        {
            foreach (var assemblyLine in assemblyLines)
            {
                var first = assemblyLine.Nodes.First();
                var last = assemblyLine.Nodes.Last();

                if (currentGridPosition == first)
                    return (AppendMode.InsertFront, assemblyLine);
                
                if (currentGridPosition == last)
                    return (AppendMode.Append, assemblyLine);
            }

            var line = new AssemblyLine(new GameObject());
            
            line.OnAssemblyLineNodeAdded += OnAssemblyLineNodeAdded;

            assemblyLines.Add(line);
            return (AppendMode.Append, line);
        }

        private void OnAssemblyLineNodeAdded(AssemblyLine assemblyLine)
        {
            // remove every child in gameobject so it can be build from ground up
            // this is a temporary solution
            foreach (Transform child in assemblyLine.AttachedGameObject.transform)
                Destroy(child.gameObject);
            
            
            for (int i = 0; i < assemblyLine.Nodes.Count - 1; i++)
            {
                HashSet<Vector3Int> points = new();

                var first = assemblyLine.Nodes[i];
                var second = assemblyLine.Nodes[i + 1];

                var direction = second - first;

                // get all discrete integer points between first and second moving like in a manhattan distance
                for (int j = 0; j <= Mathf.Abs(direction.x); j++)
                    points.Add(new Vector3Int(first.x + j * Math.Sign(direction.x), first.y, first.z));

                for (int j = 0; j <= Mathf.Abs(direction.y); j++)
                    points.Add(new Vector3Int(second.x, first.y + j * Math.Sign(direction.y), first.z));

                for (int j = 0; j <= Mathf.Abs(direction.z); j++)
                    points.Add(new Vector3Int(second.x, second.y, first.z + j * Math.Sign(direction.z)));


                var listPoints = points.ToList();

                Vector3Int previousLocalDirection = default;
                
                for (int j = 0; j < listPoints.Count; j++)
                {
                    var point = listPoints[j];
                    var nextPoint = j == listPoints.Count - 1 ? listPoints[j - 1] : listPoints[j + 1];

                    var localDirection = j == listPoints.Count - 1 ? point - nextPoint : nextPoint - point;
                    var straight = j == 0 || j == listPoints.Count - 1 || previousLocalDirection == localDirection;
                    
                    var selectedPrefab = straight ? assemblyLinePrefab : assemblyLineCornerPrefab;

                    var rotation = Quaternion.LookRotation(localDirection);
                    
                    var nextLocalDirection = j >= listPoints.Count - 2 ? localDirection : listPoints[j + 2] - nextPoint;
                    
                    if (straight)
                        rotation *= Quaternion.Euler(0, rotationOffset, 0);
                    
                    previousLocalDirection = localDirection;
                    
                    var current = Instantiate(selectedPrefab, point + placementOffset, rotation, assemblyLine.AttachedGameObject.transform);
                    current.name = $"AssemblyLine {point}";
                }
            }
        }
    }
}

public static class ExtensionMethods {
    public static String Print<T>(this List<T> list)
    {
        StringBuilder builder = new StringBuilder("[");

        for (int i = 0; i < list.Count - 1; i++)
        {
            builder.Append(list[i]);
            builder.Append(", ");
        }
        
        builder.Append(list[^1]);

        return builder.ToString();
    }
}