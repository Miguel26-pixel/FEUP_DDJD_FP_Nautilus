using System;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    public int numPointsPerAxis = 24;
    private ComputeBuffer pointsBuffer;

    public List<ProcessingStep> processingSteps;

<<<<<<< HEAD
    public ComputeBuffer Generate(Vector3 centre, float boundsSize, int seed)
    {
        pointsBuffer = new ComputeBuffer(numPointsPerAxis * numPointsPerAxis * numPointsPerAxis, sizeof(float) * 4);
        foreach (var step in processingSteps)
        {
            step.Process(pointsBuffer, numPointsPerAxis, seed, boundsSize, centre);
        }

        return pointsBuffer;
=======
    public ProcessingResult Generate(Vector3 centre, float boundsSize, int seed)
    {
        pointsBuffer = new ComputeBuffer(numPointsPerAxis * numPointsPerAxis * numPointsPerAxis, sizeof(float) * 4);
        ProcessingResult result = new ProcessingResult(new float[numPointsPerAxis, numPointsPerAxis]);
        
        foreach (var step in processingSteps)
        {
            step.Process(pointsBuffer, numPointsPerAxis, seed, boundsSize, centre, result);
        }

        result.pointsBuffer = pointsBuffer;
        return result;
>>>>>>> main
    }

    public void ReleaseBuffers()
    {
        pointsBuffer.Release();
    }
}


[Serializable]
public struct BiomeParameters
{
    public float lacunarity;
    public float persistence;
    public float noiseScale;
    public float noiseWeight;
    public float floorOffset;
    public float weightMultiplier;
    public float hardFloor;
    public float hardFloorWeight;
    public float hardCeil;
    public float hardCeilWeight;
    public float warpEffect;
    public float warpFrequency;
    public Vector3 offset;
    public Vector4 shaderParams;
}
