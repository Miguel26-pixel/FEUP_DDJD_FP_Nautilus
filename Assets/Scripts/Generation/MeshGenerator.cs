using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshGenerator : MonoBehaviour
{
    public float isoLevel;
    public float boundsSize = 1;
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
    
    private void Update()
    {
        if ((lastPosition - player.position).sqrMagnitude < generationConfigs.sqrCheckDistanceInterval) return;

        lastPosition = player.position;
        UpdateTerrain();
    }

    private void UpdateTerrain()
    {
        Vector3Int playerChunkPosition = ChunkPosition();

        if (playerChunkPosition.Equals(lastChunkPosition)) return;
        lastChunkPosition = playerChunkPosition;

        UpdateChunks(playerChunkPosition);
    }

    private Vector3Int ChunkPosition()
    {
        var position = player.transform.position;
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
                    if (_chunks.ContainsKey(chunkPosition))
                    {
                        currentChunk = _chunks[chunkPosition];
                        currentChunk.gameObject.SetActive(true);
                    }
                    else
                    {
                        GameObject chunkObject = Instantiate(chunkPrefab, chunksParent.transform);
                        chunkObject.name = $"Chunk {x} {y} {z}";
                        currentChunk = chunkObject.GetComponent<Chunk>();
                        currentChunk.chunkGridPosition = new Vector3Int(x, y, z);
                        currentChunk.Generate(isoLevel, boundsSize);
                        _chunks[chunkPosition] = currentChunk;
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
                    chunk.Generate(isoLevel, boundsSize);
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