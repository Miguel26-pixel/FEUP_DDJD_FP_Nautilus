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

    public GameObject chunksParent;
    
    public int numChunksX = 3;
    public int numChunksY = 3;
    public int numChunksZ = 1;

    private void Update()
    {
        
    }

    private void Awake()
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
                    chunk.chunkGridPosition = new Vector3(x, y, z);
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