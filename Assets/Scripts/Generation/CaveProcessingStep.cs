using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveProcessingStep : ProcessingStep
{
    public BiomeParameters caveParameters;
    public float mixA;
    public float mixB;
    public float biomeScale;

    public ComputeShader shader;

    public override void Process(ComputeBuffer pointsBuffer, int numPointsPerAxis, int seed, float boundsSize, Vector3 centre)
    {
        shader.SetInt("octaves", 6);
        shader.SetFloat("lacunarity", caveParameters.lacunarity);
        shader.SetFloat("persistence", caveParameters.persistence);
        shader.SetFloat("noiseScale", caveParameters.noiseScale);
        shader.SetFloat("noiseWeight", caveParameters.noiseWeight);
        shader.SetFloat("floorOffset", caveParameters.floorOffset);
        shader.SetFloat("weightMultiplier", caveParameters.weightMultiplier);
        shader.SetFloat("hardFloor", caveParameters.hardFloor);
        shader.SetFloat("hardFloorWeight", caveParameters.hardFloorWeight);
        shader.SetFloat("hardCeil", caveParameters.hardCeil);
        shader.SetFloat("hardCeilWeight", caveParameters.hardCeilWeight);
        shader.SetVector("offset", new Vector4(caveParameters.offset.x,caveParameters.offset.y,caveParameters.offset.z, 0));
        shader.SetVector("params", caveParameters.shaderParams);
        shader.SetFloat("biomeScale", biomeScale);
        shader.SetInt("biomeSeed", seed);
        shader.SetFloat("boundsSize", boundsSize);
        shader.SetVector("centre", centre * boundsSize);
        shader.SetFloat("spacing", boundsSize / (numPointsPerAxis - 1));
        shader.SetInt("numPointsPerAxis", numPointsPerAxis);
        shader.SetFloat("mixA", mixA);
        shader.SetFloat("mixB", mixB);
        
        
        var prng = new System.Random(seed);

        var offsets = new Vector3[6];
        float offsetRange = 1000;
        for (int i = 0; i < 6; i++) {
            offsets[i] = new Vector3 ((float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1) * offsetRange;
        }

        ComputeBuffer offsetsBuffer = new ComputeBuffer (offsets.Length, sizeof (float) * 3);
        offsetsBuffer.SetData (offsets);

        shader.SetBuffer(0, "offsets", offsetsBuffer);
        shader.SetBuffer(0, "points", pointsBuffer);

        var numThreads = Mathf.CeilToInt(numPointsPerAxis / 8f);

        shader.Dispatch(0, numThreads, numThreads, numThreads);

        offsetsBuffer.Release();
    }
}
