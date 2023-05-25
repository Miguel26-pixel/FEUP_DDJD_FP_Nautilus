using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProcessingStep : MonoBehaviour
{
    public abstract void Process(ComputeBuffer pointsBuffer, int numPointsPerAxis, int seed, float boundsSize,
        Vector3 centre, ProcessingResult result);
}

public record ProcessingResult
{
    public float[,] biomeNoise;
    public ComputeBuffer pointsBuffer;

    public ProcessingResult(float[,] biomeNoise)
    {
        this.biomeNoise = biomeNoise;
    }
}
