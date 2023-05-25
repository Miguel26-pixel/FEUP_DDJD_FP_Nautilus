using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeProcessingStep : ProcessingStep
{
    public List<BiomeParameters> biomeParameters;
    public List<float> biomesValues;
    public float biomeScale;

    public Vector3 initPos = Vector3.zero;
    public float islandRadius = 10;
    public float islandFalloff = 10;

    public ComputeShader shader;

    public override void Process(ComputeBuffer pointsBuffer, int numPointsPerAxis, int seed, float boundsSize, Vector3 centre)
    {
        var size = 19 * sizeof(float);
        ComputeBuffer biomeParametersBuffer = new ComputeBuffer(biomeParameters.Count, size);
        biomeParametersBuffer.SetData(biomeParameters);
        ComputeBuffer biomeValuesBuffer = new ComputeBuffer(biomesValues.Count, sizeof(float));
        biomeValuesBuffer.SetData(biomesValues);

        shader.SetBuffer(0, "biomes", biomeParametersBuffer);
        shader.SetBuffer(0, "biomesValues", biomeValuesBuffer);
        shader.SetInt("numBiomes", biomeParameters.Count);
        shader.SetFloat("biomeScale", biomeScale);
        shader.SetFloat("boundsSize", boundsSize);
        shader.SetVector("centre", centre * boundsSize);
        shader.SetFloat("spacing", boundsSize / (numPointsPerAxis - 1));
        shader.SetInt("numPointsPerAxis", numPointsPerAxis);
        shader.SetFloat("falloff", islandFalloff);
        shader.SetFloat("radius", islandRadius);
        shader.SetVector("initPos", initPos);
        
        var prng = new System.Random(seed);

        var offsets = new Vector3[8];
        float offsetRange = 1000;
        for (int i = 0; i < 8; i++) {
            offsets[i] = new Vector3 ((float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1) * offsetRange;
        }
        
        prng = new System.Random(seed);
        Vector3 biomeOffset = new Vector3 ((float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1) * offsetRange;
        shader.SetVector("biomeOffset", biomeOffset);

        ComputeBuffer offsetsBuffer = new ComputeBuffer (offsets.Length, sizeof (float) * 3);
        offsetsBuffer.SetData (offsets);

        shader.SetBuffer(0, "offsets", offsetsBuffer);
        shader.SetBuffer(0, "points", pointsBuffer);

        var numThreads = Mathf.CeilToInt(numPointsPerAxis / 8f);

        shader.Dispatch(0, numThreads, numThreads, numThreads);
        
        offsetsBuffer.Release();
        biomeParametersBuffer.Release();
        biomeValuesBuffer.Release();
    }
}
