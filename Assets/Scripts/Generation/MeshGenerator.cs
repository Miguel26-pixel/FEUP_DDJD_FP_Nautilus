using System;
using System.Collections;
using System.Collections.Generic;
using Generation.Resource;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshGenerator : MonoBehaviour, IDisposable
{
    public float isoLevel;
    public float boundsSize = 1;
    public int numPointsPerAxis = 16;
    public int seed = 0;
    public Transform player;

    public GameObject chunkPrefab;
    public GenerationConfigs generationConfigs;
    public ResourceGeneratorSettings[] resourceGeneratorConfigs;

    public GameObject chunksParent;
    
    public int numChunksX = 3;
    public int numChunksY = 3;
    public int numChunksZ = 1;

    private readonly Dictionary<Vector3Int, Chunk> _chunks = new();
    private readonly List<Chunk> _activeChunks = new();

    public Vector3 lastPosition = Vector3.positiveInfinity;
    public Vector3Int lastChunkPosition = new (Int32.MaxValue, Int32.MaxValue, Int32.MaxValue);
    
    public PointsGeneratorMono pointsGeneratorMono;
    public ResourceGenerator resourceGenerator;
    
    public ComputeShader terraformShader;

    public HashSet<Vector3Int> Terraform(Vector3 position, float weight, float radius)
    {
        // RWStructuredBuffer<float> modifiedNoise;
        // int3 centre;
        // int radius;
        // int numPointsPerAxis;
        // float weight;
        // float deltaTime;
        // TODO: GET NEIGHBOURS
        Vector3Int chunkPosition = ChunkPosition(position);
        Chunk chunk = _chunks[chunkPosition];

        Vector3Int chunkIndexes = chunk.GetVectorPosition(position);
        
        terraformShader.SetBuffer(0, "modifiedNoise", chunk.GetModifiedNoiseBuffer());
        terraformShader.SetInts("centre", chunkIndexes.x, chunkIndexes.y, chunkIndexes.z);
        terraformShader.SetInt("radius", Mathf.RoundToInt(radius));
        terraformShader.SetInt("numPointsPerAxis", numPointsPerAxis);
        terraformShader.SetFloat("weight", weight);
        terraformShader.SetFloat("deltaTime", Time.deltaTime);
        
        int numThreadsPerAxis = Mathf.CeilToInt(numPointsPerAxis / 8f);
        terraformShader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        return new HashSet<Vector3Int>() { chunkPosition };
    }

    public void RegenerateChunk(Vector3Int position)
    {
        Chunk chunk = _chunks[position];
        chunk.Regenerate(isoLevel);
    }
    
    private void Update()
    {
        if ((lastPosition - player.position).sqrMagnitude < generationConfigs.sqrCheckDistanceInterval) return;

        lastPosition = player.position;
        UpdateTerrain();
    }

    private void UpdateTerrain()
    {
        Vector3Int playerChunkPosition = ChunkPosition(player.transform.position);

        if (playerChunkPosition.Equals(lastChunkPosition)) return;
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

        for (int x = position.x - generationConfigs.chunkDistanceRadius; x < position.x + generationConfigs.chunkDistanceRadius; x++)
        {
            for (int y = position.y - generationConfigs.chunkDistanceRadius; y < position.y + generationConfigs.chunkDistanceRadius; y++)
            {
                for (int z = position.z - generationConfigs.chunkDistanceRadius; z < position.z + generationConfigs.chunkDistanceRadius; z++)
                {
                    Vector3Int chunkPosition = new Vector3Int(x, y, z);
                    Chunk currentChunk;
                    if (_chunks.TryGetValue(chunkPosition, out var chunk))
                    {
                        currentChunk = chunk;
                        currentChunk.gameObject.SetActive(true);
                    }
                    else
                    {
                        GameObject chunkObject = Instantiate(chunkPrefab, chunksParent.transform);
                        chunkObject.name = $"Chunk {x} {y} {z}";
                        currentChunk = chunkObject.GetComponent<Chunk>();
                        currentChunk.chunkGridPosition = new Vector3Int(x, y, z);
                        ProcessingResult result = currentChunk.Generate(isoLevel, boundsSize, numPointsPerAxis, seed);

                        float[] biomePoints = new float[numPointsPerAxis * numPointsPerAxis]; 
                        Vector3[] biomeNoise = new Vector3[numPointsPerAxis * numPointsPerAxis];
                        result.biomeBuffer.GetData(biomeNoise);
        
                        for (int i = 0; i < biomeNoise.Length; i++)
                        {
                            float yPos = biomeNoise[i].y;
                            float xPos = biomeNoise[i].x;
                            float biome = biomeNoise[i].z;
            
                            float cellSize = boundsSize / (numPointsPerAxis - 1);
            
                            int yIndex = Mathf.FloorToInt((yPos - z * boundsSize + boundsSize / 2) / cellSize);
                            int xIndex = Mathf.FloorToInt((xPos - x * boundsSize + boundsSize / 2) / cellSize);
                            biomePoints[yIndex * numPointsPerAxis  + xIndex] = biome;
                        }

                        result.biomeBuffer.Release();
                        _chunks[chunkPosition] = currentChunk;
                        currentChunk.colorGenerator.UpdateColors(seed);
                        LinkedList<Vector2>[] points = pointsGeneratorMono.pointsGenerator.GeneratePoints(new Vector2Int(x, z));
                        
                        resourceGenerator.GenerateResources(currentChunk, biomePoints, points);
                    }

                    _activeChunks.Add(currentChunk);
                }
            }
        }
        
        resourceGenerator.UpdateResources(_activeChunks);
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
                    chunk.Generate(isoLevel, boundsSize, numPointsPerAxis, seed);
                }
            }
        }
    }

    public void Dispose()
    {
        foreach (var chunkPair in _chunks)
        {
            chunkPair.Value.Dispose();
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