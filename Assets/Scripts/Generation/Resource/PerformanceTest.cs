using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generation.Resource
{
    public class PerformanceTest : MonoBehaviour
    {
        public Chunk chunk;
        public float boundsSize;
        public LayerMask layerMask;
        public MeshFilter meshFilter;

        public bool start;

        private void OnValidate()
        {
            if (start == true)
            {
                Begin();
            }
        }
        
        private void Begin()
        {
            int testIterations = 1;
            int step = 1;

            Vector3 min = new Vector3(
                chunk.chunkGridPosition.x * boundsSize - boundsSize / 2,
                chunk.chunkGridPosition.y * boundsSize - boundsSize / 2,
                chunk.chunkGridPosition.z * boundsSize - boundsSize / 2
            );
            min = chunk.transform.TransformPoint(min);
            
            Vector3 max = new Vector3(
                chunk.chunkGridPosition.x * boundsSize + boundsSize / 2,
                chunk.chunkGridPosition.y * boundsSize + boundsSize / 2,
                chunk.chunkGridPosition.z * boundsSize + boundsSize / 2
            );
            max = chunk.transform.TransformPoint(max);

            Debug.Log("Starting triangle traversal test...");
            System.Diagnostics.Stopwatch triangleTraversalTimer = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < testIterations; i++)
            {
                // TriangleTraversal(min.x, max.x, min.z, max.z, step);
            }
            triangleTraversalTimer.Stop();
            Debug.Log("Triangle traversal test completed in " + triangleTraversalTimer.ElapsedMilliseconds + " ms");

            // Start raycasting test
            Debug.Log("Starting raycasting test...");
            System.Diagnostics.Stopwatch raycastingTimer = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < testIterations; i++)
            {
                Raycasting(min.x, max.x, min.z, max.z, step);
            }
            raycastingTimer.Stop();
            Debug.Log("Raycasting test completed in " + raycastingTimer.ElapsedMilliseconds + " ms");
        }
        
        private void TriangleTraversal(float minX, float maxX, float minZ, float maxZ, int step)
        {
            TriangleSurfacePointsFinder finder = new TriangleSurfacePointsFinder(meshFilter);

            List<Vector2> search = new List<Vector2>();
            for (float x = minX; x < maxX; x += step)
            {
                for (float z = minZ; z < maxZ; z += step)
                {
                    search.Add(new Vector2(x, z));
                }
            }

            Vector3[] points = finder.FindUpwardSurfacePoints(search.ToArray());
        }
        
        private void Raycasting(float minX, float maxX, float minZ, float maxZ, int step)
        {
            RaycastSurfacePointsFinder finder = new RaycastSurfacePointsFinder(chunk, layerMask, boundsSize);

            for (float x = minX; x < maxX; x += step)
            {
                for (float z = minZ; z < maxZ; z += step)
                {
                    Vector3[] points = finder.FindUpwardSurfacePoints(x, z);

                    foreach (var point in points)
                    {
                        Debug.Log(x + ", " + point.y + ", " + z);

                        Debug.DrawLine(point, point + Vector3.up * 20, Color.red, 10000);
                    }
                }
            }
        }
    }
}