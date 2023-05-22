using System;
using UnityEngine;

namespace Generation.Resource
{
    public class RaycastSurfacePointsFinder : MonoBehaviour
    {
        public Chunk chunk;
        public Vector2 point;
        public float boundsSize;
        public LayerMask layerMask;
        
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
                Debug.Log("test");
                Debug.DrawLine(new Vector3(point.x, 10, point.y), new Vector3(point.x, 10, point.y) + Vector3.down * 3, Color.yellow);

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
            if (chunk == null) return Array.Empty<Vector3>();
            Vector3 rayStart = new Vector3(x, 
                chunk.chunkGridPosition.y * boundsSize + boundsSize / 2 + 5, 
                z);

            return Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, boundsSize * 1.2f, layerMask) ? new[] { hit.point } : Array.Empty<Vector3>();
        }
    }
}