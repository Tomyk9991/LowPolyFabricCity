using System.Collections.Generic;
using System.Linq;
using Common.Managers;
using Unity.VisualScripting;
using UnityEngine;
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
    
    public class AssemblyLineManager : Manager
    {
        private readonly List<AssemblyLine> assemblyLines = new();

        [Header("Assembly line settings")]
        [SerializeField] private GameObject assemblyLinePrefab;
        [SerializeField] private GameObject assemblyLineCornerPrefab;
        

        [SerializeField] private float rotationOffset = 90.0f;
        [SerializeField] private Vector3 placementOffset = new(-0.5f, 0, -0.5f);

        [Header("Debug")]
        [SerializeField] private Transform targetA;
        [SerializeField] private Transform targetB;

        public void Awake()
        {
            AssemblyLine.rotationOffset = this.rotationOffset;
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
            
            // remove every child in gameobject so it can be build from ground up
            // this is a temporary solution
            foreach (Transform child in assemblyLine.AttachedGameObject.transform)
                Destroy(child.gameObject);

            var meshesCount = assemblyLine.Connections.Sum(t => t.DiscretePoints.Count);
            
            MeshFilter[] meshFilters = new MeshFilter[meshesCount];
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];
            Mesh[] meshInstances = {
                assemblyLinePrefab.GetComponent<MeshFilter>().sharedMesh,
                assemblyLineCornerPrefab.GetComponent<MeshFilter>().sharedMesh
            };
            
            int i = 0;
            foreach (Connection connection in assemblyLine.Connections)
            {
                foreach (PointRotation pointRotation in connection.DiscretePoints)
                {
                    var point = pointRotation.Point;
                    var finalRotation = pointRotation.Rotation;
                    int prefabInstanceIndex = 0;
                    
                    if (pointRotation.IsCorner)
                        prefabInstanceIndex = 1;
                    
                    combine[i].mesh = meshInstances[prefabInstanceIndex];
                    combine[i].transform = Matrix4x4.TRS(point + placementOffset, finalRotation, Vector3.one);
                    
                    // var current = Instantiate(prefabInstanceIndex, point + placementOffset, finalRotation, assemblyLine.AttachedGameObject.transform);
                    // current.name = $"AssemblyLine {point}";
                    i++;
                }
            }


            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combine);
            
            if (assemblyLine.AttachedGameObject.GetComponent<MeshFilter>() == null)
                assemblyLine.AttachedGameObject.AddComponent<MeshFilter>();
            
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