using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generation.Resource
{
    public class ResourceGenerator : MonoBehaviour
    {
        public LayerMask layerMask;
        // public GenerationConfigs generationConfigs;    
        public GameObject test;
        
        private RaycastSurfacePointsFinder _pointsFinder;

        private void Start()
        {
            MeshGenerator generator = GameObject.Find("GenerationManager").GetComponent<MeshGenerator>();
            _pointsFinder = new RaycastSurfacePointsFinder(layerMask, generator.boundsSize);
        }

        public void GenerateResources(Chunk chunk, float[,] biomeNoise , LinkedList<Vector2>[] points)
        {
            for (int i = 0; i < points.Length; i++)
            {
                foreach (var point in points[i])
                {
                    Vector3[] surfacePoints = _pointsFinder.FindUpwardSurfacePoints(chunk, point.x, point.y);
                    
                    foreach (var surfacePoint in surfacePoints)
                    {
                        Instantiate(test, surfacePoint, Quaternion.identity);
                    }
                }
            }
        }
    }
}