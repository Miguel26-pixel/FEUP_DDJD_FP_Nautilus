using UnityEngine;
using Random = System.Random;

public class CaveProcessingStep : ProcessingStep
{
    public BiomeParameters caveParameters;
    public float mixA;
    public float mixB;
    public float biomeScale;

    public ComputeShader shader;

    private bool _initialized;
    private ComputeBuffer _offsetsBuffer;

    private void InitializeBuffers(float boundsSize, int numPointsPerAxis, int seed)
    {
        if (_initialized)
        {
            return;
        }

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
        shader.SetVector("offset",
            new Vector4(caveParameters.offset.x, caveParameters.offset.y, caveParameters.offset.z, 0));
        shader.SetVector("params", caveParameters.shaderParams);
        shader.SetFloat("biomeScale", biomeScale);
        shader.SetFloat("boundsSize", boundsSize);
        shader.SetFloat("spacing", boundsSize / (numPointsPerAxis - 1));
        shader.SetFloat("mixA", mixA);
        shader.SetFloat("mixB", mixB);
        shader.SetInt("numPointsPerAxis", numPointsPerAxis);

        Random prng = new Random(seed);

        Vector3[] offsets = new Vector3[6];
        float offsetRange = 1000;
        for (int i = 0; i < 6; i++)
        {
            offsets[i] = new Vector3((float)prng.NextDouble() * 2 - 1, (float)prng.NextDouble() * 2 - 1,
                (float)prng.NextDouble() * 2 - 1) * offsetRange;
        }

        prng = new Random(seed);
        Vector3 biomeOffset = new Vector3((float)prng.NextDouble() * 2 - 1, (float)prng.NextDouble() * 2 - 1,
            (float)prng.NextDouble() * 2 - 1) * offsetRange;
        shader.SetVector("biomeOffset", biomeOffset);

        _offsetsBuffer = new ComputeBuffer(offsets.Length, sizeof(float) * 3);
        _offsetsBuffer.SetData(offsets);

        shader.SetBuffer(0, "offsets", _offsetsBuffer);
        _initialized = true;
    }

    public override void Process(ComputeBuffer pointsBuffer, int numPointsPerAxis, int seed, float boundsSize,
        Vector3 centre, ProcessingResult result)
    {
        InitializeBuffers(boundsSize, numPointsPerAxis, seed);

        shader.SetVector("centre", centre * boundsSize);
        shader.SetBuffer(0, "points", pointsBuffer);

        int numThreads = Mathf.CeilToInt(numPointsPerAxis / 8f);

        shader.Dispatch(0, numThreads, numThreads, numThreads);
    }

    public override void Dispose()
    {
        _offsetsBuffer?.Release();
    }
}