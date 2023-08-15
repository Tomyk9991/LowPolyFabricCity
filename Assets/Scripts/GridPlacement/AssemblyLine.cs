using System;
using System.Collections.Generic;
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
        public Vector3 Rotation { get; set; }

        public PointRotation(Vector3Int point, Vector3 rotation)
        {
            this.Point = point;
            this.Rotation = rotation;
        }
    }
    
    public class AssemblyLine
    {
        public List<Vector3Int> Nodes { get; } = new();
        public List<PointRotation> DiscretePoints { get; set; } = new();
        
        /// <summary>
        /// Indicates, if the assembly line is finished building or not
        /// </summary>
        public bool Finished { get; set; }
        
        public GameObject AttachedGameObject { get; private set; }
        
        public event Action<AssemblyLine, bool> OnAssemblyLineNodeAdded;
        
        public AssemblyLine(GameObject gameObject)
        {
            AttachedGameObject = gameObject;
            Finished = false;
        }
        
        public void AddNode(Vector3Int currentGridPosition, bool solutionA)
        {
            if (!Nodes.Contains(currentGridPosition))
                Nodes.Add(currentGridPosition);
            
            OnAssemblyLineNodeAdded?.Invoke(this, solutionA);
        }

        public void InsertFront(Vector3Int currentGridPosition, bool solutionA)
        {
            if (!Nodes.Contains(currentGridPosition))
                Nodes.Insert(0, currentGridPosition);

            OnAssemblyLineNodeAdded?.Invoke(this, solutionA);
        }
    }
}