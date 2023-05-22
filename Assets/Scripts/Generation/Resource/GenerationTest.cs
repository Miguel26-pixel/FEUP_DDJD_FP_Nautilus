using System.Collections;
using System.Collections.Generic;
using Generation.Resource;
using UnityEngine;

public class GenerationTest : MonoBehaviour
{
    public float radius = 1;
    public Vector2 sampleRegionSize = Vector2.one;
    public int numSamplesBeforeRejection = 30;

    private List<Vector2> points;
    
    public float displayRadius = 1;

    private void OnValidate()
    {
        points = PoissonDiscSampling.GeneratePoints(radius, sampleRegionSize, numSamplesBeforeRejection);
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(sampleRegionSize/2, sampleRegionSize);
        if (points != null)
        {
            foreach (Vector2 point in points)
            {
                Gizmos.DrawSphere(point, displayRadius);
            }
        }
    }
}
