using System;
using System.Collections;
using System.Collections.Generic;
using Generation.Resource;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshGenerator : MonoBehaviour
{
    public float isoLevel;
    public float boundsSize = 1;
    public int seed = 0;
    public Transform player;

    public GameObject chunkPrefab;
    public GenerationConfigs generationConfigs;

    public GameObject chunksParent;
    
    public int numChunksX = 3;
    public int numChunksY = 3;
    public int numChunksZ = 1;

    private readonly Dictionary<Vector3Int, Chunk> _chunks = new();
    private readonly List<Chunk> _activeChunks = new();

    public Vector3 lastPosition = Vector3.positiveInfinity;
    public Vector3Int lastChunkPosition = new (Int32.MaxValue, Int32.MaxValue, Int32.MaxValue);
    
    public ResourceGeneratorMono resourceGeneratorMono;

    public Chunk[] getChunksAt(Vector2 position, int minY, int maxY)
    {
        Vector3Int chunkPosition = ChunkPosition(
            new Vector3(position.x, 0, position.y));
        
        List<Chunk> chunks = new List<Chunk>();
        
        for (int y = minY; y <= maxY; y++)
        {
            Vector3Int chunkPositionY = chunkPosition;
            chunkPositionY.y = y;

            
            
            if (_chunks.TryGetValue(chunkPositionY, out var chunk))
            {
                if (chunkPositionY == new Vector3Int(4, -1, 0))
                {
                    Debug.Log("test");
                }
                
                chunks.Add(chunk);
            }
        }
        
        return chunks.ToArray();
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
                        ProcessingResult result = currentChunk.Generate(isoLevel, boundsSize, seed);
                        _chunks[chunkPosition] = currentChunk;
                        currentChunk.colorGenerator.UpdateColors(seed);
                        resourceGeneratorMono.resourceGenerator.GeneratePoints(new Vector2Int(x, z));
                    }

                    _activeChunks.Add(currentChunk);
                }
            }
        }
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
                    chunk.Generate(isoLevel, boundsSize, seed);
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