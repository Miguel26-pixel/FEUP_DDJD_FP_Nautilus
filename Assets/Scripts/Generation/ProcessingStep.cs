using System;
using UnityEngine;

public abstract class ProcessingStep : MonoBehaviour, IDisposable
{
    private void OnDestroy()
    {
        Dispose();
    }

    public abstract void Dispose();

    public abstract void Process(ComputeBuffer pointsBuffer, int numPointsPerAxis, int seed, float boundsSize,
        Vector3 centre, ProcessingResult result);
}

public record ProcessingResult
{
    public ComputeBuffer biomeBuffer;
    public ComputeBuffer pointsBuffer;
}