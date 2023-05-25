using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generation.Resource
{
    public class TriangleSurfacePointsFinder
    {
        private readonly MeshFilter _meshFilter;

        public TriangleSurfacePointsFinder(MeshFilter meshFilter)
        {
            this._meshFilter = meshFilter;
        }

        public Vector3[] FindUpwardSurfacePoints(Vector2[] search)
        {
             if (_meshFilter == null) return Array.Empty<Vector3>();
            Mesh mesh = _meshFilter.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            
            List<Vector3> points = new List<Vector3>();

            Vector3[] relativePoints = new Vector3[search.Length];
            
            for (int i = 0; i < search.Length; i++)
            {
                Vector2 point = search[i];
                Vector3 relativePoint = _meshFilter.transform.InverseTransformPoint(new Vector3(point.x, 0, point.y));
                relativePoints[i] = relativePoint;
            }

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

                    foreach (var relativePoint in relativePoints)
                    {
                        (float u, float v, float w) = BarycentricCoordinates(new Vector2(relativePoint.x, relativePoint.z), v1xz, v2xz, v3xz);
                    
                        // Check if the point is inside the triangle
                        if (PointInTriangle(u, v, w))
                        {
                            // Calculate barycentric coordinates of the point relative to the triangle
                            // Get the height of the triangle at the point
                            float height = GetHeightAtPoint(u, v, w, v1.y, v1.y, v1.y);
                        
                            // Get the point in world space
                            Vector3 result = new Vector3(relativePoint.x, height, relativePoint.z);
                            result = _meshFilter.transform.TransformPoint(result);
                        
                            // Add the point to the list
                            points.Add(result);
                        }
                    }
                }
            }
            
            return points.ToArray();
        }

        public Vector3[] FindUpwardSurfacePoints(float x, float z)
        {
            return FindUpwardSurfacePoints(new Vector2[]{ new Vector2(x, z) });
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