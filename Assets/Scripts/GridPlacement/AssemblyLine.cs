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
    
    public class AssemblyLine
    {
        public List<Vector3Int> Nodes { get; } = new();
        public GameObject AttachedGameObject { get; private set; }
        
        public event Action<AssemblyLine> OnAssemblyLineNodeAdded;
        
        public AssemblyLine(GameObject gameObject)
        {
            AttachedGameObject = gameObject;
        }
        
        public void AddNode(Vector3Int currentGridPosition)
        {
            if (!Nodes.Contains(currentGridPosition))
                Nodes.Add(currentGridPosition);
            
            OnAssemblyLineNodeAdded?.Invoke(this);
        }

        public void InsertFront(Vector3Int currentGridPosition)
        {
            if (!Nodes.Contains(currentGridPosition))
                Nodes.Insert(0, currentGridPosition);

            OnAssemblyLineNodeAdded?.Invoke(this);
        }
    }
}