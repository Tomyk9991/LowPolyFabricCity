using ExtensionMethods;
using UnityEngine;

public class SetGradientToLineRenderer : MonoBehaviour
{
    [SerializeField] private Gradient gradient;

    private LineRenderer lineRenderer;
    
    private void OnValidate()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();
        
        // lineRenderer.SetGradientFixed(gradient);
    }
}
