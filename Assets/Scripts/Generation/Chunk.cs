using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class Chunk : MonoBehaviour
{
    public Vector3Int chunkGridPosition;
    public ColorGenerator colorGenerator;
    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;
    
    private ComputeBuffer triangleBuffer;
    private ComputeBuffer triCountBuffer;
    public ComputeShader shader;
    private NoiseGenerator _noiseGenerator;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
        _noiseGenerator = GetComponent<NoiseGenerator>();
        colorGenerator = GetComponent<ColorGenerator>();
    }

    private void GenerateCollider()
    {
        _meshCollider.sharedMesh = _meshFilter.sharedMesh;
    }

    private ProcessingResult GenerateNoise(float boundsSize, int seed)
    {
        return _noiseGenerator.Generate(chunkGridPosition, boundsSize, seed);
    }

    private Triangle[] GenerateTriangles(float isoLevel, ComputeBuffer pointsBuffer)
    {
        int numVoxelsPerAxis = _noiseGenerator.numPointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;

        triangleBuffer = new ComputeBuffer (maxTriangleCount, sizeof (float) * 9, ComputeBufferType.Append);
        triCountBuffer = new ComputeBuffer (1, sizeof (int), ComputeBufferType.Raw);

        shader.SetInt("numPointsPerAxis", _noiseGenerator.numPointsPerAxis);
        shader.SetFloat("isoLevel", isoLevel);
        shader.SetBuffer(0, "points", pointsBuffer);
        shader.SetBuffer(0, "triangles", triangleBuffer);

        int numThreadsPerAxis = Mathf.CeilToInt (numVoxelsPerAxis / 8f);
        shader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        _noiseGenerator.ReleaseBuffers();

        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);

        int numTriangles = triCountArray[0];

        Triangle[] triangles = new Triangle[numTriangles];
        triangleBuffer.GetData(triangles);

        ReleaseBuffers();

        return triangles;
    }

    private void ReleaseBuffers()
    {
        triangleBuffer.Release();
        triCountBuffer.Release();
    }

    private void GenerateMesh(Triangle[] triangles)
    {
        Mesh mesh = new();
        mesh.Clear();
        mesh.indexFormat = IndexFormat.UInt32;
        _meshFilter.sharedMesh = mesh;
        
        Vector3[] vertices = new Vector3[triangles.Length * 3];
        int[] meshTriangles = new int[triangles.Length * 3];

        for (int i = 0; i < triangles.Length; i++) {
            for (int j = 0; j < 3; j++) {
                meshTriangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = triangles[i][j];
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;

        mesh.RecalculateNormals();
    }

    public ProcessingResult Generate(float isoLevel, float chunkSize, int seed)
    {
        ProcessingResult result = GenerateNoise(chunkSize, seed);
        Triangle[] triangles = GenerateTriangles(isoLevel, result.pointsBuffer);
        GenerateMesh(triangles);
        GenerateCollider();

        return result;
    }
    
    public Vector3 GetWorldPosition(float boundsSize)
    {
        Vector3 local = new Vector3(
            chunkGridPosition.x * boundsSize,
            chunkGridPosition.y * boundsSize,
            chunkGridPosition.z * boundsSize);
        
        return transform.TransformPoint(local);
    }
}
