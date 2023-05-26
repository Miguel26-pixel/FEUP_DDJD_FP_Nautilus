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
    public ComputeBuffer biomeBuffer;
    public ComputeBuffer pointsBuffer;
}
