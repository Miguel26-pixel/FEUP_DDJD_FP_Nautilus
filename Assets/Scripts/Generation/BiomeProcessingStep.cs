using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeProcessingStep : ProcessingStep, IDisposable
{
    public List<BiomeParameters> biomeParameters;
    public List<float> biomesValues;
    public float biomeScale;

    public Vector3 initPos = Vector3.zero;
    public float islandRadius = 10;
    public float islandFalloff = 10;

    public ComputeShader shader;
    
    private ComputeBuffer _biomeParametersBuffer;
    private ComputeBuffer _biomeValuesBuffer;
    private ComputeBuffer _biomeNoiseBuffer;
    private ComputeBuffer _offsetsBuffer;
    
    private bool _initialized = false;

    private void InitializeBuffers(int seed, float boundsSize, int numPointsPerAxis)
    {
        if (_initialized)
        {
            return;
        }

        const int size = 19 * sizeof(float); 
        _biomeParametersBuffer = new ComputeBuffer(biomeParameters.Count, size);
        _biomeParametersBuffer.SetData(biomeParameters);
        _biomeValuesBuffer = new ComputeBuffer(biomesValues.Count, sizeof(float));
        _biomeValuesBuffer.SetData(biomesValues);
        _biomeNoiseBuffer = new ComputeBuffer(numPointsPerAxis * numPointsPerAxis, sizeof(float) * 3);
        
        shader.SetBuffer(0, "biomeNoiseB", _biomeNoiseBuffer);
        shader.SetBuffer(0, "biomes", _biomeParametersBuffer);
        shader.SetBuffer(0, "biomesValues", _biomeValuesBuffer);
        
        var prng = new System.Random(seed);

        var offsets = new Vector3[8];
        float offsetRange = 1000;
        for (int i = 0; i < 8; i++) {
            offsets[i] = new Vector3 ((float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1) * offsetRange;
        }
        
        prng = new System.Random(seed);
        Vector3 biomeOffset = new Vector3 ((float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1, (float) prng.NextDouble () * 2 - 1) * offsetRange;
        shader.SetVector("biomeOffset", biomeOffset);

        _offsetsBuffer = new ComputeBuffer (offsets.Length, sizeof (float) * 3);
        _offsetsBuffer.SetData (offsets);

        shader.SetBuffer(0, "offsets", _offsetsBuffer);
        
        shader.SetInt("numBiomes", biomeParameters.Count);
        shader.SetFloat("biomeScale", biomeScale);
        shader.SetFloat("boundsSize", boundsSize);
        shader.SetInt("numPointsPerAxis", numPointsPerAxis);
        shader.SetFloat("spacing", boundsSize / (numPointsPerAxis - 1));

        shader.SetFloat("falloff", islandFalloff);
        shader.SetFloat("radius", islandRadius);
        shader.SetVector("initPos", initPos);

        _initialized = true;
    }

    public override void Process(ComputeBuffer pointsBuffer, int numPointsPerAxis, int seed, float boundsSize, Vector3 centre, ProcessingResult result)
    {
        InitializeBuffers(seed, boundsSize, numPointsPerAxis);
        
        shader.SetVector("centre", centre * boundsSize);
        shader.SetBuffer(0, "points", pointsBuffer);

        var numThreads = Mathf.CeilToInt(numPointsPerAxis / 8f);

        shader.Dispatch(0, numThreads, numThreads, numThreads);
        result.biomeBuffer = _biomeNoiseBuffer;
    }

    public override void Dispose()
    {
        _biomeParametersBuffer?.Release();
        _biomeValuesBuffer?.Release();
        _biomeNoiseBuffer?.Release();
        _offsetsBuffer?.Release();
    }
}
