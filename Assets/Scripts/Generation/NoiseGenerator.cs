using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

public class NoiseGenerator : MonoBehaviour
{
    [Header("Noise")] 
    public int seed;
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
    public int numPointsPerAxis = 24;
    public float boundsSize = 1;
    public Vector3 centre = new(0,0,0);
    public Vector3 offset = new(0,0,0);
    public Vector4 shaderParams = new(1, 0, 0, 0);
    
    public ComputeShader noiseShader;
    private ComputeBuffer offsetsBuffer;
    private ComputeBuffer pointsBuffer;

    public ComputeBuffer Generate()
    {
        noiseShader.SetInt("octaves", numOctaves);
        noiseShader.SetFloat("lacunarity", lacunarity);
        noiseShader.SetFloat("persistence", persistence);
        noiseShader.SetFloat("noiseScale", noiseScale);
        noiseShader.SetFloat("noiseWeight", noiseWeight);
        noiseShader.SetFloat("floorOffset", floorOffset);
        noiseShader.SetFloat("weightMultiplier", weightMultiplier);
        noiseShader.SetFloat("hardFloor", hardFloor);
        noiseShader.SetFloat("hardFloorWeight", hardFloorWeight);
        noiseShader.SetFloat("warpEffect", warpEffect);
        noiseShader.SetFloat("warpFrequency", warpFrequency);
        noiseShader.SetInt("numPointsPerAxis", numPointsPerAxis);
        noiseShader.SetFloat("boundsSize", boundsSize);
        noiseShader.SetVector("centre", new Vector4(centre.x,centre.y,centre.z, 0));
        noiseShader.SetVector("offset", new Vector4(offset.x,offset.y,offset.z, 0));
        noiseShader.SetVector("params", shaderParams);
        noiseShader.SetFloat("spacing", boundsSize / (numPointsPerAxis - 1));

        var prng = new System.Random(seed);
        var offsets = new Vector3[numOctaves];
        float offsetRange = 1000;
        for (int i = 0; i < numOctaves; i++) {
            offsets[i] = new Vector3 ((float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1) * offsetRange;
        }

        offsetsBuffer = new ComputeBuffer (offsets.Length, sizeof (float) * 3);
        offsetsBuffer.SetData (offsets);

        noiseShader.SetBuffer(0, "offsets", offsetsBuffer);

        pointsBuffer = new ComputeBuffer(numPointsPerAxis * numPointsPerAxis * numPointsPerAxis, sizeof(float) * 4);
        noiseShader.SetBuffer(0, "points", pointsBuffer);

        var numThreads = Mathf.CeilToInt(numPointsPerAxis / 8f);
        noiseShader.Dispatch(0, numThreads, numThreads, numThreads);

        Vector4[] points = new Vector4[numPointsPerAxis * numPointsPerAxis * numPointsPerAxis];
        pointsBuffer.GetData(points, 0, 0, numPointsPerAxis ^ 3);

        return pointsBuffer;
    }

    public void ReleaseBuffers()
    {
        offsetsBuffer.Release();
        pointsBuffer.Release();
    }
}
