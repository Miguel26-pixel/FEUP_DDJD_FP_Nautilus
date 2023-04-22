using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

public class NoiseGenerator : MonoBehaviour
{
    [Header("Noise")] 

    public int numPointsPerAxis = 24;
    private ComputeBuffer offsetsBuffer;
    private ComputeBuffer pointsBuffer;
    public List<BiomeParameters> biomeParameters;

    public ComputeBuffer Generate(Vector3 centre, float boundsSize)
    {
        var prng = new System.Random(1);

        pointsBuffer = new ComputeBuffer(numPointsPerAxis * numPointsPerAxis * numPointsPerAxis, sizeof(float) * 4);

        foreach (var biomeParameter in biomeParameters)
        {
            biomeParameter.UpdateValues(boundsSize, centre, numPointsPerAxis);
            var offsets = new Vector3[biomeParameter.numOctaves];
            float offsetRange = 1000;
            for (int i = 0; i < biomeParameter.numOctaves; i++) {
                offsets[i] = new Vector3 ((float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1) * offsetRange;
            }

            offsetsBuffer = new ComputeBuffer (offsets.Length, sizeof (float) * 3);
            offsetsBuffer.SetData (offsets);

            biomeParameter.shader.SetBuffer(0, "offsets", offsetsBuffer);
            biomeParameter.shader.SetBuffer(0, "points", pointsBuffer);

            var numThreads = Mathf.CeilToInt(numPointsPerAxis / 8f);
            biomeParameter.shader.Dispatch(0, numThreads, numThreads, numThreads);

            offsetsBuffer.Release();
        }

        Vector4[] points = new Vector4[numPointsPerAxis * numPointsPerAxis * numPointsPerAxis];
        pointsBuffer.GetData(points, 0, 0, numPointsPerAxis ^ 3);

        return pointsBuffer;
    }

    public void ReleaseBuffers()
    {
        pointsBuffer.Release();
    }
}


[Serializable]
public class BiomeParameters
{
    public int numOctaves = 4;
    public float lacunarity = 2;
    public float persistence = .5f;
    public float noiseScale = 1;
    public float noiseWeight = 1;
    public float floorOffset = 1;
    public float weightMultiplier = 1;
    public float hardFloor;
    public float hardFloorWeight;
    public float warpEffect;
    public float warpFrequency = 0.004f;
    public Vector3 offset = new(0,0,0);
    public Vector4 shaderParams = new(1, 0, 0, 0);

    public ComputeShader shader;

    public void UpdateValues(float boundsSize, Vector4 centre, int numPointsPerAxis)
    {
        shader.SetInt("octaves", numOctaves);
        shader.SetFloat("lacunarity", lacunarity);
        shader.SetFloat("persistence", persistence);
        shader.SetFloat("noiseScale", noiseScale);
        shader.SetFloat("noiseWeight", noiseWeight);
        shader.SetFloat("floorOffset", floorOffset);
        shader.SetFloat("weightMultiplier", weightMultiplier);
        shader.SetFloat("hardFloor", hardFloor);
        shader.SetFloat("hardFloorWeight", hardFloorWeight);
        shader.SetFloat("warpEffect", warpEffect);
        shader.SetFloat("warpFrequency", warpFrequency);
        shader.SetInt("numPointsPerAxis", numPointsPerAxis);
        shader.SetFloat("boundsSize", boundsSize);
        shader.SetVector("centre", centre * boundsSize);
        shader.SetVector("offset", new Vector4(offset.x,offset.y,offset.z, 0));
        shader.SetVector("params", shaderParams);
        shader.SetFloat("spacing", boundsSize / (numPointsPerAxis - 1));
    }
}
