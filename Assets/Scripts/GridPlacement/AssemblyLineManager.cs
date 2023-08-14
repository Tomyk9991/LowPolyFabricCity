using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace GridPlacement
{
    public enum Direction
    {
        East,
        South,
        West,
        North,
    }
    
    public class AssemblyLineManager : MonoBehaviour
    {
        private readonly List<AssemblyLine> assemblyLines = new();

        [Header("Assembly line settings")]
        [SerializeField] private GameObject assemblyLinePrefab;
        [SerializeField] private GameObject assemblyLineCornerPrefab;
        

        [SerializeField] private float rotationOffset = 90.0f;
        [SerializeField] private Vector3 placementOffset = new(-0.5f, 0, -0.5f);

        [Header("Debug")]
        [SerializeField] private Transform targetA = null;
        [SerializeField] private Transform targetB = null;
        


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

        private void OnAssemblyLineNodeAdded(AssemblyLine assemblyLine, bool solutionA)
        {
            // remove every child in gameobject so it can be build from ground up
            // this is a temporary solution
            foreach (Transform child in assemblyLine.AttachedGameObject.transform)
                Destroy(child.gameObject);

            if (assemblyLine.Nodes.Count == 1)
            {
                var point = assemblyLine.Nodes[0];
                var current = Instantiate(assemblyLinePrefab, point + placementOffset, Quaternion.identity, assemblyLine.AttachedGameObject.transform);
                current.name = $"AssemblyLine {point}";

                assemblyLine.DiscretePoints = new List<PointRotation>
                {
                    new(point, Quaternion.identity.eulerAngles)
                };
            }
            
            var allPoints = new List<PointRotation>();
            for (int i = 0; i < assemblyLine.Nodes.Count - 1; i++)
            {
                var first = assemblyLine.Nodes[i];
                var second = assemblyLine.Nodes[i + 1];

                var listPoints = solutionA ? GetPointsBetweenV1(first, second) : GetPointsBetweenV2(first, second);

                for (int j = 0; j < listPoints.Count; j++)
                {
                    var point = listPoints[j];
                    var nextPoint = j == listPoints.Count - 1 ? listPoints[j - 1] : listPoints[j + 1];
                    
                    var localDirection = j == listPoints.Count - 1 ? point - nextPoint : nextPoint - point;
                    var direction = GetCardinalDirection(Quaternion.LookRotation(localDirection));

                    Debug.Log(direction);
                    
                    var finalRotation = Quaternion.identity;

                    for (int k = 0; k < (int) direction; k++)
                        finalRotation *= Quaternion.Euler(0, rotationOffset, 0);
                    
                    var current = Instantiate(assemblyLinePrefab, point + placementOffset, finalRotation, assemblyLine.AttachedGameObject.transform);
                    current.name = $"AssemblyLine {point}";
                }

                var filtered = from tuple in allPoints select tuple.Point;
                
                foreach (var point in listPoints.Where(point => !filtered.Contains(point)))
                    allPoints.Add(new PointRotation(point, Quaternion.identity.eulerAngles));
            }

            assemblyLine.DiscretePoints = allPoints;
        }

        private static Direction GetCardinalDirection(Quaternion rotation)
        {
            Vector3 forward = rotation * Vector3.forward;
            float angle = Vector3.SignedAngle(Vector3.forward, forward, Vector3.up);

            return angle switch
            {
                >= -45f and < 45f => Direction.North,
                >= 45f and < 135f => Direction.East,
                >= -135f and < -45f => Direction.West,
                _ => Direction.South
            };
        }
        
        public static List<Vector3Int> GetPointsBetweenV1(Vector3Int first, Vector3Int second)
        {
            List<Vector3Int> points = new List<Vector3Int>();
            
            var direction = second - first;
            
            // get all discrete integer points between first and second moving like in a manhattan distance
            for (int j = 0; j <= Mathf.Abs(direction.x); j++)
            {
                var target = new Vector3Int(first.x + j * Math.Sign(direction.x), first.y, first.z);
                if (!points.Contains(target))
                    points.Add(target);
            }

            for (int j = 0; j <= Mathf.Abs(direction.y); j++)
            {
                var target = new Vector3Int(second.x, first.y + j * Math.Sign(direction.y), first.z);
                if (!points.Contains(target))
                    points.Add(target);
            }

            for (int j = 0; j <= Mathf.Abs(direction.z); j++)
            {
                var target = new Vector3Int(second.x, second.y, first.z + j * Math.Sign(direction.z));
                if (!points.Contains(target))
                    points.Add(target);
            }

            return points;
        }
        
        public static List<Vector3Int> GetPointsBetweenV2(Vector3Int first, Vector3Int second)
        {
            List<Vector3Int> points = new List<Vector3Int>();
            var direction = second - first;
            
            // get all discrete integer points between first and second moving like in a manhattan distance
            for (int j = 0; j <= Mathf.Abs(direction.z); j++)
            {
                var target = new Vector3Int(first.x, second.y, first.z + j * Math.Sign(direction.z));
                if (!points.Contains(target))
                    points.Add(target);
            }

            for (int j = 0; j <= Mathf.Abs(direction.y); j++)
            {
                var target = new Vector3Int(first.x, first.y + j * Math.Sign(direction.y), first.z);
                if (!points.Contains(target))
                    points.Add(target);
            }

            for (int j = 0; j <= Mathf.Abs(direction.x); j++)
            {
                var target = new Vector3Int(first.x + j * Math.Sign(direction.x), second.y, second.z);
                if (!points.Contains(target))
                    points.Add(target);
            }

            return points;
        }

        private void OnDrawGizmos()
        {
            if (targetA == null || targetB == null) return;
            
            var a = targetA.position;
            var b = targetB.position;
            
            var solutionA = GetPointsBetweenV1(
                new Vector3Int(Mathf.CeilToInt(a.x), Mathf.CeilToInt(a.y), Mathf.CeilToInt(a.z)), 
                new Vector3Int(Mathf.CeilToInt(b.x), Mathf.CeilToInt(b.y), Mathf.CeilToInt(b.z))
            );
            
            var solutionB = GetPointsBetweenV2(
                new Vector3Int(Mathf.CeilToInt(a.x), Mathf.CeilToInt(a.y), Mathf.CeilToInt(a.z)), 
                new Vector3Int(Mathf.CeilToInt(b.x), Mathf.CeilToInt(b.y), Mathf.CeilToInt(b.z))
            );

            Gizmos.color = Color.red;
            foreach (var point in solutionA)
            {
                Gizmos.DrawWireCube(point, Vector3.one * 0.45f);
            }


            Gizmos.color = Color.green;
            foreach (var point in solutionB)
            {
                Gizmos.DrawWireCube(point, Vector3.one * 0.5f);
            }
        }
    }
}