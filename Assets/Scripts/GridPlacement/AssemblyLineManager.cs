using System.Collections.Generic;
using System.Linq;
using Common.Managers;
using UnityEngine;

namespace GridPlacement
{
    public enum Direction
    {
        East,
        South,
        West,
        North,
    }
    
    public class AssemblyLineManager : Manager
    {
        private readonly List<AssemblyLine> assemblyLines = new();

        [Header("Assembly line settings")]
        [SerializeField] private GameObject assemblyLinePrefab;
        [SerializeField] private GameObject assemblyLineCornerPrefab;
        [SerializeField] private Material assemblyLineMaterial;
        
        

        [SerializeField] private float rotationOffset = 90.0f;
        [SerializeField] private Vector3 placementOffset = new(-0.5f, 0, -0.5f);

        [Header("Debug")]
        [SerializeField] private Transform targetA;
        [SerializeField] private Transform targetB;

        private Mesh[][] meshInstances;

        public void Awake()
        {
            AssemblyLine.rotationOffset = rotationOffset;
        }

        private void Start()
        {
            meshInstances = new[]
            {
                assemblyLinePrefab.GetComponentsInChildren<MeshFilter>().Select(t => t.sharedMesh).ToArray(),
                assemblyLineCornerPrefab.GetComponentsInChildren<MeshFilter>().Select(t => t.sharedMesh).ToArray()
            };
        }
        

        public (AppendMode, AssemblyLine) GetOrCreate(Vector3Int currentGridPosition)
        {
            foreach (var assemblyLine in assemblyLines)
            {
                var first = assemblyLine.Connections[0].Start;
                var last = assemblyLine.Connections[^1].End;

                if (currentGridPosition == first)
                    return (AppendMode.InsertFront, assemblyLine);
                
                if (currentGridPosition == last)
                    return (AppendMode.Append, assemblyLine);
            }
            
            var line = new AssemblyLine(new GameObject
            {
                name = $"AssemblyLine {currentGridPosition}"
            });
            line.OnAssemblyLineNodeAdded += OnAssemblyLineNodeAdded;

            assemblyLines.Add(line);
            return (AppendMode.Append, line);
        }

        private void OnAssemblyLineNodeAdded(AssemblyLine assemblyLine)
        {
            if (!assemblyLine.Finished) return;
            
            List<CombineInstance> combine = new();
            
            foreach (Connection connection in assemblyLine.Connections)
            {
                foreach (PointRotation pointRotation in connection.DiscretePoints)
                {
                    var point = pointRotation.Point;
                    var finalRotation = pointRotation.Rotation;
                    finalRotation *= Quaternion.Euler(-90.0f, 0.0f, 0.0f);
                    
                    int prefabInstanceIndex = 0;
                    
                    if (pointRotation.IsCorner)
                        prefabInstanceIndex = 1;


                    for (int j = 0; j < meshInstances[prefabInstanceIndex].Length; j++)
                    {
                        CombineInstance instance = new()
                        {
                            mesh = meshInstances[prefabInstanceIndex][j],
                            transform = Matrix4x4.TRS(point + placementOffset, finalRotation, Vector3.one * 100)
                        };
                        
                        combine.Add(instance);
                    }
                }
            }


            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combine.ToArray());
            
            if (assemblyLine.AttachedGameObject.GetComponent<MeshFilter>() == null)
                assemblyLine.AttachedGameObject.AddComponent<MeshFilter>();

            if (assemblyLine.AttachedGameObject.GetComponent<MeshRenderer>() == null)
            {
                var r = assemblyLine.AttachedGameObject.AddComponent<MeshRenderer>();
                r.material = assemblyLineMaterial;
            }
            
            assemblyLine.AttachedGameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
        }
        
        private void OnDrawGizmos()
        {
            if (targetA == null || targetB == null) return;
            
            var a = targetA.position;
            var b = targetB.position;
            
            var solutionA = AssemblyLine.GetPointsBetweenV1(
                new Vector3Int(Mathf.CeilToInt(a.x), Mathf.CeilToInt(a.y), Mathf.CeilToInt(a.z)), 
                new Vector3Int(Mathf.CeilToInt(b.x), Mathf.CeilToInt(b.y), Mathf.CeilToInt(b.z))
            );
            
            var solutionB = AssemblyLine.GetPointsBetweenV2(
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