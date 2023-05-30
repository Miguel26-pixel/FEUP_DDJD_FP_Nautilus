using System;
using System.Collections.Generic;
using System.Linq;
using Generation.Resource;
using UnityEngine;
using Utils;

public class MeshGenerator : MonoBehaviour, IDisposable
{
    public float isoLevel;
    public float boundsSize = 1;
    public int numPointsPerAxis = 16;
    public int seed;
    public Transform player;

    public GameObject chunkPrefab;
    public GenerationConfigs generationConfigs;
    public ResourceGeneratorSettings[] resourceGeneratorConfigs;

    public GameObject chunksParent;

    public int numChunksX = 3;
    public int numChunksY = 3;
    public int numChunksZ = 1;

    public Vector3 lastPosition = Vector3.positiveInfinity;
    public Vector3Int lastChunkPosition = new(int.MaxValue, int.MaxValue, int.MaxValue);

    public PointsGeneratorMono pointsGeneratorMono;
    public ResourceGenerator resourceGenerator;

    public ComputeShader terraformShader;
    private readonly List<Chunk> _activeChunks = new();
    private readonly Queue<Chunk> _toGenerateChunks = new();

    private readonly Dictionary<Vector3Int, Chunk> _chunks = new();
    private ColorGenerator _colorGenerator;
    private NoiseGenerator _noiseGenerator;
    
    private bool _isInitialized;

    private void Start()
    {
        _noiseGenerator = GetComponent<NoiseGenerator>();
        _colorGenerator = GetComponent<ColorGenerator>();
        _isInitialized = false;
    }

    private void Update()
    {
        GenerateChunks();
        
        if ((lastPosition - player.position).sqrMagnitude < generationConfigs.sqrCheckDistanceInterval)
        {
            return;
        }

        lastPosition = player.position;
        UpdateTerrain();
    }

    public void OnDestroy()
    {
        Dispose();
    }

    public void Dispose()
    {
        foreach (KeyValuePair<Vector3Int, Chunk> chunkPair in _chunks)
        {
            chunkPair.Value.Dispose();
        }

        _noiseGenerator.Dispose();
    }

    private void GenerateChunk(Chunk currentChunk)
    {
          
        ProcessingResult result = currentChunk.Generate(isoLevel, boundsSize, numPointsPerAxis,
            _noiseGenerator, seed);

        float[] biomePoints = new float[numPointsPerAxis * numPointsPerAxis];
        Vector3[] biomeNoise = new Vector3[numPointsPerAxis * numPointsPerAxis];
        result.biomeBuffer.GetData(biomeNoise);

        for (int i = 0; i < biomeNoise.Length; i++)
        {
            float yPos = biomeNoise[i].y;
            float xPos = biomeNoise[i].x;
            float biome = biomeNoise[i].z;

            float cellSize = boundsSize / (numPointsPerAxis - 1);

            int yIndex = Mathf.FloorToInt(
                (yPos - currentChunk.chunkGridPosition.z * boundsSize + boundsSize / 2) /
                cellSize);
            int xIndex = Mathf.FloorToInt(
                (xPos - currentChunk.chunkGridPosition.x * boundsSize + boundsSize / 2) /
                cellSize);
            biomePoints[yIndex * numPointsPerAxis + xIndex] = biome;
        }

        _chunks[currentChunk.chunkGridPosition] = currentChunk;
        _colorGenerator.UpdateColors(seed, currentChunk.GetMeshRenderer().material);
        LinkedList<Vector2>[] points =
            pointsGeneratorMono.pointsGenerator.GeneratePoints(new Vector2Int(
                currentChunk.chunkGridPosition.x,
                currentChunk.chunkGridPosition.z));

        resourceGenerator.GenerateResources(currentChunk, biomePoints, points);
    }
    
    private void GenerateChunks()
    {
        if (_toGenerateChunks.Count <= 0)
        {
            return;
        }
        
        Chunk currentChunk = _toGenerateChunks.Dequeue();
        GenerateChunk(currentChunk);
      
        HashSet<Vector3Int> generatedChunks = new HashSet<Vector3Int>();

        foreach (var chunk in _activeChunks)
        {
            if (chunk.Generated)
            {
                generatedChunks.Add(chunk.chunkGridPosition);
            }
        }

        resourceGenerator.UpdateResources(generatedChunks);
    }

    public HashSet<Vector3Int> Terraform(Vector3 position, float weight, float radius)
    {
        Vector3Int chunkPosition = ChunkPosition(position);
        HashSet<Vector3Int> modifiedChunks = new();

        for (int x = chunkPosition.x - 1; x <= chunkPosition.x + 1; x++)
        {
            for (int y = chunkPosition.y - 1; y <= chunkPosition.y + 1; y++)
            {
                for (int z = chunkPosition.z - 1; z <= chunkPosition.z + 1; z++)
                {
                    Vector3 localPosition = chunksParent.transform.InverseTransformPoint(position);
                    if (!MathUtils.SphereIntersectsBox(localPosition, radius, new Vector3(x, y, z) * boundsSize,
                            new Vector3(boundsSize, boundsSize, boundsSize)))
                    {
                        continue;
                    }

                    Chunk chunk = _chunks[new Vector3Int(x, y, z)];

                    Vector3Int chunkIndexes = chunk.GetVectorPosition(position);

                    terraformShader.SetBuffer(0, "modifiedNoise", chunk.GetModifiedNoiseBuffer());
                    terraformShader.SetInts("centre", chunkIndexes.x, chunkIndexes.y, chunkIndexes.z);
                    terraformShader.SetInt("radius", chunk.GetFloatDistance(radius));
                    terraformShader.SetInt("numPointsPerAxis", numPointsPerAxis);
                    terraformShader.SetFloat("weight", weight);
                    terraformShader.SetFloat("deltaTime", Time.deltaTime);

                    int numThreadsPerAxis = Mathf.CeilToInt(numPointsPerAxis / 8f);
                    terraformShader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);
                    chunk.Regenerate(isoLevel);
                    modifiedChunks.Add(chunk.chunkGridPosition);
                }
            }
        }

        return modifiedChunks;
    }

    public void RegenerateChunk(Vector3Int position)
    {
        Chunk chunk = _chunks[position];
        chunk.RenderMesh();

        resourceGenerator.CheckGrounded(position);
    }

    private void UpdateTerrain()
    {
        Vector3Int playerChunkPosition = ChunkPosition(player.transform.position);

        if (playerChunkPosition.Equals(lastChunkPosition))
        {
            return;
        }

        lastChunkPosition = playerChunkPosition;

        UpdateChunks(playerChunkPosition);
    }

    private Vector3Int ChunkPosition(Vector3 position)
    {
        position = chunksParent.transform.InverseTransformPoint(position);

        return new Vector3Int(
            Mathf.RoundToInt(position.x / boundsSize),
            Mathf.RoundToInt(position.y / boundsSize),
            Mathf.RoundToInt(position.z / boundsSize)
        );
    }

    private void UpdateChunks(Vector3Int position)
    {
        _activeChunks.ForEach(c => c.gameObject.SetActive(false));
        _activeChunks.Clear();
        HashSet<Vector3Int> activeChunkPositions = new();

        for (int x = position.x - generationConfigs.chunkDistanceRadius;
             x < position.x + generationConfigs.chunkDistanceRadius;
             x++)
        {
            for (int y = position.y - generationConfigs.chunkDistanceRadius;
                 y < position.y + generationConfigs.chunkDistanceRadius;
                 y++)
            {
                for (int z = position.z - generationConfigs.chunkDistanceRadius;
                     z < position.z + generationConfigs.chunkDistanceRadius;
                     z++)
                {
                    Vector3Int chunkPosition = new(x, y, z);
                    Chunk currentChunk;
                    if (_chunks.TryGetValue(chunkPosition, out Chunk chunk))
                    {
                        currentChunk = chunk;
                        currentChunk.gameObject.SetActive(true);
                        activeChunkPositions.Add(chunkPosition);
                    }
                    else
                    {
                        GameObject chunkObject = Instantiate(chunkPrefab, chunksParent.transform);
        
                        chunkObject.name = $"Chunk {x} {y} {z}";
                        currentChunk = chunkObject.GetComponent<Chunk>();
                        currentChunk.chunkGridPosition = new Vector3Int(x, y, z);

                        if (_isInitialized)
                        {
                            _toGenerateChunks.Enqueue(currentChunk);
                        }
                        else
                        {
                            GenerateChunk(currentChunk);
                            activeChunkPositions.Add(chunkPosition);
                        }
                    }

                    _activeChunks.Add(currentChunk);
                }
            }
        }

        _isInitialized = true;
        resourceGenerator.UpdateResources(activeChunkPositions);
    }

    private void GenerateFixedChunks()
    {
        for (int x = 0; x < numChunksX; x++)
        {
            for (int y = 0; y < numChunksY; y++)
            {
                for (int z = 0; z < numChunksZ; z++)
                {
                    GameObject chunkObject = Instantiate(chunkPrefab, chunksParent.transform);
                    chunkObject.name = $"Chunk {x} {y} {z}";
                    Chunk chunk = chunkObject.GetComponent<Chunk>();
                    chunk.chunkGridPosition = new Vector3Int(x, y, z);

                    chunk.Generate(isoLevel, boundsSize, numPointsPerAxis, _noiseGenerator, seed);
                }
            }
        }
    }
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