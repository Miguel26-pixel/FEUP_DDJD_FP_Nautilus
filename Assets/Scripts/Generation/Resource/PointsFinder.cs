using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generation.Resource
{
    public class PointsFinder : MonoBehaviour
    {
        public Chunk chunk;
        public Vector2 point;
        public float boundsSize;
        public LayerMask layerMask;
        public MeshFilter meshFilter;
        
        private bool settingsChanged;

        private void OnValidate()
        {
            settingsChanged = true;
        }

        private void Update()
        {
            if (settingsChanged)
            {
                Vector3[] points = FindUpwardSurfacePoints(point.x, point.y);

                foreach (Vector3 p in points)
                {
                    Debug.DrawLine(p, p + Vector3.up * 20, Color.red, 10000);
                    Debug.Log(p);
                }

                settingsChanged = false;
            }
        }

        public Vector3[] FindUpwardSurfacePoints(float x, float z)
        {
            if (chunk == null) return new Vector3[]{};
            RaycastSurfacePointsFinder finder = new RaycastSurfacePointsFinder(chunk, layerMask, boundsSize);
            return finder.FindUpwardSurfacePoints(x, z);
        }
    }
}