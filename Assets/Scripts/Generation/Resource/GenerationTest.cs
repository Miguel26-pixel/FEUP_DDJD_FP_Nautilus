using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Generation.Resource;
using Unity.VisualScripting;
using UnityEngine;

public class GenerationTest : MonoBehaviour
{
    public float radius = 1;
    public Vector2 sampleRegionSize = Vector2.one;
    public int numSamplesBeforeRejection = 30;
    public LayerMask layerMask;
    public float boundsSize;
    public MeshGenerator meshGenerator;
    
    private List<Vector2> points;
    private List<Vector3> pointsWorld;

    public int minChunk;
    public int maxChunk;
    
    public float displayRadius = 1;

    private RaycastSurfacePointsFinder raycastSurfacePointsFinder;

    private void OnValidate() 
    {
        raycastSurfacePointsFinder = new RaycastSurfacePointsFinder(layerMask, boundsSize);

        points = PoissonDiscSampling.GeneratePoints(radius, sampleRegionSize, numSamplesBeforeRejection);
        
        pointsWorld = new List<Vector3>();
        foreach (Vector2 point in points)
        {
            Chunk[] chunks = meshGenerator.getChunksAt(point, minChunk, maxChunk);

            foreach (Chunk chunk in chunks)
            {
                if (chunk.chunkGridPosition == new Vector3Int(4, -1, 0))
                {
                    Debug.Log("test");
                }
                pointsWorld.Add(raycastSurfacePointsFinder.FindUpwardSurfacePoints(chunk, point.x, point.y).FirstOrDefault());
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(sampleRegionSize/2, sampleRegionSize);
        if (pointsWorld != null) 
        {
            foreach (Vector3 point in pointsWorld)
            {
                Gizmos.DrawSphere(point, displayRadius);
            }
        }
    }
}
