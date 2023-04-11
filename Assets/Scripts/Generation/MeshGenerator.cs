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
    }

    private void GenerateMesh()
    {
        var triangles = GenerateTriangles();

        print(triangles[0].a);
        
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

        int numPoints = noiseGenerator.numPointsPerAxis ^ 3;
        int numVoxelsPerAxis = noiseGenerator.numPointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;

        var debug = new ComputeBuffer (maxTriangleCount, sizeof (int), ComputeBufferType.Append);
        triangleBuffer = new ComputeBuffer (maxTriangleCount, sizeof (int), ComputeBufferType.Append);
        triCountBuffer = new ComputeBuffer (1, sizeof (int), ComputeBufferType.Raw);

        shader.SetInt("numPointsPerAxis", noiseGenerator.numPointsPerAxis);
        shader.SetFloat("isoLevel", isoLevel);
        shader.SetBuffer(0, "points", pointsBuffer);
        shader.SetBuffer(0, "triangles", triangleBuffer);
        shader.SetBuffer(0, "debug", debug);

        var numThreads = Mathf.CeilToInt(numVoxelsPerAxis / 8f);
        shader.Dispatch(0, numThreads, numThreads, numThreads);

        noiseGenerator.ReleaseBuffers();

        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);

        int numTriangles = triCountArray[0];

        Triangle[] triangles = new Triangle[numTriangles];
        triangleBuffer.GetData(triangles);

        int[] values = new int[1001];
        debug.GetData(values);

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
