using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GridPlacement
{
    public enum AppendMode 
    {
        Append,
        InsertFront
    }

    public class PointRotation
    {
        public Vector3Int Point { get; set; }
        public Quaternion Rotation { get; set; }
        public bool IsCorner { get; set; }
        
        public PointRotation(Vector3Int point, Quaternion rotation, bool isCorner)
        {
            this.Point = point;
            this.Rotation = rotation;
            this.IsCorner = isCorner;
        }
    }


    public class Connection
    {
        public Vector3Int Start { get; set; }
        public Vector3Int End { get; set; }
        public bool SolutionA { get; set; }
        
        public List<PointRotation> DiscretePoints { get; set; } = new();
    }
    
    public class AssemblyLine
    {
        public static float rotationOffset = 90.0f;
        // public List<Vector3Int> Nodes { get; } = new();
        public List<Connection> Connections { get; } = new();
        
        /// <summary>
        /// Indicates, if the assembly line is finished building or not
        /// </summary>
        public bool Finished { get; set; }
        
        public GameObject AttachedGameObject { get; private set; }
        
        public event Action<AssemblyLine> OnAssemblyLineNodeAdded;
        
        public AssemblyLine(GameObject gameObject)
        {
            AttachedGameObject = gameObject;
            Finished = false;
        }
        
        public void AddConnection(Vector3Int start, Vector3Int end, bool solutionA)
        {
            if (Connections.Any(t => t.Start == start && t.End == end)) return;
            if (Connections.Any(t => t.Start == end && t.End == start)) return;
            
            Connections.Add(new Connection
            {
                Start = start,
                End = end,
                SolutionA = solutionA
            });

            CalculateDiscretePoints();
            OnAssemblyLineNodeAdded?.Invoke(this);
        }
        
        public void InsertFrontConnection(Vector3Int start, Vector3Int end, bool solutionA)
        {
            if (Connections.Any(t => t.Start == start && t.End == end)) return;
            if (Connections.Any(t => t.Start == end && t.End == start)) return;
            
            Connections.Insert(0, new Connection
            {
                Start = start,
                End = end,
                SolutionA = solutionA
            });

            CalculateDiscretePoints();
            OnAssemblyLineNodeAdded?.Invoke(this);
        }

        public void CalculateDiscretePoints()
        {
            if (!Finished) return;
            
            if (Connections.Count == 1 && Connections[0].Start == Connections[0].End)
            {
                var point = Connections[0].Start;
                Connections[0].DiscretePoints = new List<PointRotation>
                {
                    new(point, Quaternion.identity, false)
                };
                
                return;
            }
            
            foreach (var connection in Connections)
            {
                var allPoints = new List<PointRotation>();
                
                var first = connection.Start;
                var second = connection.End;
                var listPoints = connection.SolutionA ? GetPointsBetweenV1(first, second) : GetPointsBetweenV2(first, second);

                Vector3Int? previousDirection = null;
                
                for (int i = 0; i < listPoints.Count; i++)
                {
                    var point = listPoints[i];
                    var nextPoint = i == listPoints.Count - 1 ? listPoints[i - 1] : listPoints[i + 1];
                    
                    var localDirection = i == listPoints.Count - 1 ? point - nextPoint : nextPoint - point;
                    var lookRotation = Quaternion.LookRotation(localDirection);
                    var finalRotation = Quaternion.identity;
                    
                    // corner
                    bool isCorner = previousDirection.HasValue && previousDirection != localDirection;

                    var rotation = 0;
                    if (isCorner)
                    {
                        var lr = Quaternion.LookRotation(localDirection);
                        var angle = Vector3.SignedAngle((localDirection - previousDirection.Value), Vector3.right, Vector3.up);
                        var cardinalDirection = GetCardinalDirection(lr);

                        rotation = cardinalDirection switch
                        {
                            Direction.South when Mathf.Approximately(-135.0f, angle) => 3,
                            Direction.West when Mathf.Approximately(135.0f, angle) => 0,
                            Direction.North when Mathf.Approximately(45.0f, angle) => 1,
                            Direction.East when Mathf.Approximately(-45.0f, angle) => 2,
                            _ => GetCardinalDirection(lr) switch
                            {
                                Direction.North => 0,
                                Direction.East => 1,
                                Direction.South => 2,
                                Direction.West => 3,
                                _ => throw new ArgumentOutOfRangeException()
                            }
                        };
                    }
                    
                    if (!isCorner)
                    {
                        var direction = GetCardinalDirection(lookRotation);
                        rotation = (int)direction;
                    }
                    
                    for (int j = 0; j < rotation; j++)
                        finalRotation *= Quaternion.Euler(0, rotationOffset, 0);
                    
                    previousDirection = localDirection;
                    allPoints.Add(new PointRotation(point, finalRotation, isCorner));
                }
                
                connection.DiscretePoints = allPoints;
            }
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
    }
}