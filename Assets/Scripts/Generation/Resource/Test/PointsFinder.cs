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
                HitInformation[] points = FindUpwardSurfacePoints(point.x, point.y);

                foreach (var r in points)
                {
                    Vector3 p = r.position;
                    Debug.DrawLine(p, p + Vector3.up * 20, Color.red, 10000);
                    Debug.Log(p);
                }

                settingsChanged = false;
            }
        }

        public HitInformation[] FindUpwardSurfacePoints(float x, float z)
        {
            if (chunk == null) return new HitInformation[]{};
            RaycastSurfacePointsFinder finder = new RaycastSurfacePointsFinder(layerMask, boundsSize);
            return finder.FindUpwardSurfacePoints(chunk, x, z);
        }
    }
}