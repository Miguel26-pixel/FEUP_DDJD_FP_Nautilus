using System;
using UnityEngine;
using UnityEngine.Rendering;

public class Chunk : MonoBehaviour, IDisposable
{
    public Vector3Int chunkGridPosition;
    public ComputeShader shader;
    
    public bool Generated { get; private set; }
    
    private float _boundsSize;

    private bool _initialized;
    private float _isoLevel;
    private MeshCollider _meshCollider;

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    [NonSerialized] private ComputeBuffer _modifiedNoise;

    private int _numPointsPerAxis;

    // TODO: CHECK if can be just computeBuffer
    private ComputeBuffer _points;
    private int _seed;

    private ComputeBuffer _triangleBuffer;
    private int _triangleCount;
    private ComputeBuffer _triCountBuffer;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void OnDestroy()
    {
        Dispose();
    }

    public void Dispose()
    {
        _modifiedNoise?.Release();
        _points?.Release();
        _triangleBuffer?.Release();
        _triCountBuffer?.Release();
    }

    private void InitializeBuffers()
    {
        if (_initialized)
        {
            return;
        }

        int numVoxelsPerAxis = _numPointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;

        _triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 9, ComputeBufferType.Append);
        _triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        shader.SetInt("numPointsPerAxis", _numPointsPerAxis);
        shader.SetFloat("isoLevel", _isoLevel);
        shader.SetBuffer(0, "triangles", _triangleBuffer);

        _initialized = true;
    }

    private void GenerateCollider()
    {
        _meshCollider.sharedMesh = _meshFilter.sharedMesh;
    }

    private ProcessingResult GenerateNoise(float boundsSize, NoiseGenerator noiseGenerator, int seed)
    {
        return noiseGenerator.Generate(chunkGridPosition, boundsSize, _numPointsPerAxis, seed);
    }

    private void DispatchMarchingCubes(float isoLevel, ComputeBuffer pointsBuffer, ComputeBuffer modifiedNoiseBuffer)
    {
        int numVoxelsPerAxis = _numPointsPerAxis - 1;

        InitializeBuffers();
        _triangleBuffer.SetCounterValue(0);

        shader.SetBuffer(0, "points", pointsBuffer);
        shader.SetBuffer(0, "triangles", _triangleBuffer);
        shader.SetBuffer(0, "modifiedNoise", modifiedNoiseBuffer);

        int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / 8f);
        shader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        _points = pointsBuffer;
        _modifiedNoise = modifiedNoiseBuffer;
    }

    private Triangle[] GenerateTriangles()
    {
        ComputeBuffer.CopyCount(_triangleBuffer, _triCountBuffer, 0);
        int[] triCountArray = { 0 };
        _triCountBuffer.GetData(triCountArray);

        int numTriangles = triCountArray[0];

        Triangle[] triangles = new Triangle[numTriangles];
        _triangleBuffer.GetData(triangles);
        _triangleCount = numTriangles;

        ReleaseBuffers();

        return triangles;
    }

    private void ReleaseBuffers()
    {
    }

    private void GenerateMesh(Triangle[] triangles)
    {
        Mesh mesh = new();
        mesh.Clear();
        mesh.indexFormat = IndexFormat.UInt32;
        _meshFilter.sharedMesh = mesh;

        Vector3[] vertices = new Vector3[triangles.Length * 3];
        int[] meshTriangles = new int[triangles.Length * 3];

        for (int i = 0; i < triangles.Length; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                meshTriangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = triangles[i][j];
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;

        mesh.RecalculateNormals();
        _meshRenderer.shadowCastingMode = ShadowCastingMode.TwoSided;
    }

    public void Regenerate(float isoLevel)
    {
        shader.SetBool("useModifiedNoise", true);
        DispatchMarchingCubes(isoLevel, _points, _modifiedNoise);
    }

    public void RenderMesh()
    {
        Triangle[] triangles = GenerateTriangles();
        GenerateMesh(triangles);
        GenerateCollider();
    }

    private ProcessingResult DispatchShaders(float isoLevel, float chunkSize, int numPointsPerAxis,
        NoiseGenerator noiseGenerator, int seed)
    {
        _numPointsPerAxis = numPointsPerAxis;
        _seed = seed;
        _boundsSize = chunkSize;
        _isoLevel = isoLevel;

        ProcessingResult result = GenerateNoise(chunkSize, noiseGenerator, seed);
        ComputeBuffer modifiedNoiseBuffer =
            new(
                numPointsPerAxis * numPointsPerAxis * numPointsPerAxis,
                sizeof(float));

        DispatchMarchingCubes(isoLevel, result.pointsBuffer, modifiedNoiseBuffer);

        return result;
    }

    public ProcessingResult Generate(float isoLevel, float chunkSize, int numPointsPerAxis,
        NoiseGenerator noiseGenerator, int seed)
    {
        ProcessingResult result = DispatchShaders(isoLevel, chunkSize, numPointsPerAxis, noiseGenerator, seed);
        RenderMesh();
        Generated = true;

        return result;
    }

    public int GetFloatDistance(float distance)
    {
        float cellSize = _boundsSize / (_numPointsPerAxis - 1);
        int cellDistance = Mathf.RoundToInt(distance / cellSize);

        return cellDistance;
    }

    public Vector2Int GetPointPosition(Vector3 position)
    {
        Vector3 local = transform.InverseTransformPoint(position);

        float y = local.z;
        float x = local.x;

        float cellSize = _boundsSize / (_numPointsPerAxis - 1);

        int yIndex = Mathf.RoundToInt((y - chunkGridPosition.z * _boundsSize + _boundsSize / 2) / cellSize);
        int xIndex = Mathf.RoundToInt((x - chunkGridPosition.x * _boundsSize + _boundsSize / 2) / cellSize);

        return new Vector2Int(xIndex, yIndex);
    }

    public Vector3Int GetVectorPosition(Vector3 position)
    {
        Vector3 local = transform.InverseTransformPoint(position);

        float z = local.z;
        float y = local.y;
        float x = local.x;

        float cellSize = _boundsSize / (_numPointsPerAxis - 1);

        int zIndex = Mathf.RoundToInt((z - chunkGridPosition.z * _boundsSize + _boundsSize / 2) / cellSize);
        int yIndex = Mathf.RoundToInt((y - chunkGridPosition.y * _boundsSize + _boundsSize / 2) / cellSize);
        int xIndex = Mathf.RoundToInt((x - chunkGridPosition.x * _boundsSize + _boundsSize / 2) / cellSize);

        return new Vector3Int(xIndex, yIndex, zIndex);
    }

    public ComputeBuffer GetModifiedNoiseBuffer()
    {
        return _modifiedNoise;
    }

    public MeshRenderer GetMeshRenderer()
    {
        return _meshRenderer;
    }

    public Tuple<ComputeBuffer, int> GetTriangleBuffer()
    {
        return new Tuple<ComputeBuffer, int>(_triangleBuffer, _triangleCount);
    }

    public int ChunkSeed()
    {
        return (_seed + chunkGridPosition.x + chunkGridPosition.y + chunkGridPosition.z) * 31;
    }
}