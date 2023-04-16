using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshGenerator : MonoBehaviour
{
    public NoiseGenerator noiseGenerator;
    public ComputeShader shader;
    public float isoLevel;
    private ComputeBuffer triangleBuffer;
    private ComputeBuffer triCountBuffer;

    public MeshFilter meshFilter;
    
    private void Awake()
    {
        GenerateMesh();
        GenerateCollider();
    }

    private void GenerateCollider()
    {
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.sharedMesh;
    }

    private void GenerateMesh()
    {
        var triangles = GenerateTriangles();

        Mesh mesh = new();
        mesh.Clear();
        mesh.indexFormat = IndexFormat.UInt32;
        meshFilter.sharedMesh = mesh;
        
        Vector3[] vertices = new Vector3[triangles.Length * 3];
        int[] meshTriangles = new int[triangles.Length * 3];

        for (int i = 0; i < triangles.Length; i++) {
            for (int j = 0; j < 3; j++) {
                meshTriangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = triangles[i][j];
            }
            Debug.Log(i + " " + vertices[i]);
        }
        
        
        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;

        mesh.RecalculateNormals();
    }
    
    private Triangle[] GenerateTriangles()
    {
        var pointsBuffer = noiseGenerator.Generate();

        int numPoints = noiseGenerator.numPointsPerAxis * noiseGenerator.numPointsPerAxis * noiseGenerator.numPointsPerAxis;
        int numVoxelsPerAxis = noiseGenerator.numPointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;

        triangleBuffer = new ComputeBuffer (maxTriangleCount, sizeof (int), ComputeBufferType.Append);
        triCountBuffer = new ComputeBuffer (1, sizeof (int), ComputeBufferType.Raw);

        shader.SetInt("numPointsPerAxis", noiseGenerator.numPointsPerAxis);
        shader.SetFloat("isoLevel", isoLevel);
        shader.SetBuffer(0, "points", pointsBuffer);
        shader.SetBuffer(0, "triangles", triangleBuffer);

        int numThreadsPerAxis = Mathf.CeilToInt (numVoxelsPerAxis / 8f);
        shader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        noiseGenerator.ReleaseBuffers();

        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);

        int numTriangles = triCountArray[0];

        Triangle[] triangles = new Triangle[numTriangles];
        triangleBuffer.GetData(triangles);

        ReleaseBuffers();

        return triangles;
    }

    public void ReleaseBuffers()
    {
        triangleBuffer.Release();
        triCountBuffer.Release();
    }
    
    
    public struct Triangle
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return a;
                    case 1:
                        return b;
                    default:
                        return c;
                }
            }
        }
    }
}
