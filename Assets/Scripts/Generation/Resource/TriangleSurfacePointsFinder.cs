using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generation.Resource
{
    public class TriangleSurfacePointsFinder : MonoBehaviour
    {
        public MeshFilter meshFilter;
        public Vector2 point;

        private bool settingsChanges = false;
        
        private void OnValidate()
        {
            settingsChanges = true;
        }
        
        private void Update()
        {
            if (settingsChanges)
            {
                Vector3[] points = FindUpwardSurfacePoints(point.x, point.y);
                Debug.Log("test");
                
                foreach (Vector3 p in points)
                {
                    Debug.DrawLine(p, p + Vector3.up * 20, Color.red, 10000);
                    Debug.Log(p);
                }

                settingsChanges = false;
            }
        }

        public Vector3[] FindUpwardSurfacePoints(float x, float z)
        {
            if (meshFilter == null) return Array.Empty<Vector3>();
            Mesh mesh = meshFilter.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            
            List<Vector3> points = new List<Vector3>();
            
            Vector2 relativePoint = meshFilter.transform.InverseTransformPoint(new Vector2(x, z));

            for (int i = 0; i < triangles.Length; i += 3)
            {
                // Get the three vertices that make up this triangle
                Vector3 v1 = vertices[triangles[i]];
                Vector3 v2 = vertices[triangles[i + 1]];
                Vector3 v3 = vertices[triangles[i + 2]];
                
                // Get the normal of the triangle
                Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1).normalized;
                
                // Check if the triangle is facing upwards
                if (normal.y > 0)
                {
                    // Project the triangle onto the xz plane
                    Vector2 v1xz = new Vector2(v1.x, v1.z);
                    Vector2 v2xz = new Vector2(v2.x, v2.z);
                    Vector2 v3xz = new Vector2(v3.x, v3.z);
                    (float u, float v, float w) = BarycentricCoordinates(relativePoint, v1xz, v2xz, v3xz);
                    
                    // Check if the point is inside the triangle
                    if (PointInTriangle(u, v, w))
                    {
                        // Calculate barycentric coordinates of the point relative to the triangle
                        // Get the height of the triangle at the point
                        float height = GetHeightAtPoint(u, v, w, v1.y, v1.y, v1.y);
                        
                        // Get the point in world space
                        Vector3 result = new Vector3(x, height, z);
                        result = meshFilter.transform.TransformPoint(result);
                        
                        // Add the point to the list
                        points.Add(result);
                    }
                }
            }
            
            return points.ToArray();
        }
        
        private Tuple<float, float, float> BarycentricCoordinates(Vector2 p, Vector2 p1, Vector2 p2, Vector2 p3)
        {
            // Calculate barycentric coordinates of the point relative to the triangle
            float det = (p2.y - p3.y) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.y - p3.y);

            float u = ((p2.y - p3.y) * (p.x - p3.x) + (p3.x - p2.x) * (p.y - p3.y)) / det;
            float v = ((p3.y - p1.y) * (p.x - p3.x) + (p1.x - p3.x) * (p.y - p3.y)) / det;
            
            float w = 1 - u - v;

            return new Tuple<float, float, float>(u, v, w);
        }

        private bool PointInTriangle(float u, float v, float w)
        {
            // Check if the point is inside the triangle
            return u is >= 0 and <= 1 && v is >= 0 and <= 1 && w is >= 0 and <= 1;
        }

        private float GetHeightAtPoint(float u, float v, float w, float y1, float y2, float y3)
        {
            return u * y1 + v * y2 + w * y3;
        }
    }
}