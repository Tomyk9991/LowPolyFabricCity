using Attributes;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    [SerializeField] private bool aroundCenter = true;
    [DrawIfFalse(nameof(aroundCenter)), SerializeField] private float height = 0.0f;

    [DrawIfTrue(nameof(aroundCenter)), SerializeField]
    private float offsetHeight = 0.001f;
    
    [SerializeField] private Transform followTarget;

    private Vector3 cleanedTargetPosition;
    
    private void Update()
    {
        cleanedTargetPosition.Set(followTarget.position.x, aroundCenter ? offsetHeight : height, followTarget.position.z);
        transform.position = cleanedTargetPosition;
    }
}
