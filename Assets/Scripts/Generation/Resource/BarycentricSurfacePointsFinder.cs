using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generation.Resource
{
    public struct HitInformation
    {
        public Vector4 position;
        public Vector3 normal;
    }

    public class BarycentricSurfacePointsFinder : IDisposable
    {
        private readonly ComputeShader _shader;
        private ComputeBuffer _hitBuffer;
        private ComputeBuffer _hitCountBuffer;
        private ComputeBuffer _pointBuffer;

        public BarycentricSurfacePointsFinder(ComputeShader shader, int maxPoints, int maxTriangles)
        {
            _shader = shader;

            InitializeBuffers(maxPoints, maxTriangles);
        }

        public void Dispose()
        {
            _hitBuffer?.Release();
            _hitCountBuffer?.Release();
            _pointBuffer?.Release();
        }

        private void InitializeBuffers(int maxPoints, int maxTriangles)
        {
            _pointBuffer = new ComputeBuffer(maxPoints, sizeof(float) * 3);
            _hitBuffer = new ComputeBuffer(maxPoints * maxTriangles, sizeof(float) * 7, ComputeBufferType.Append);
            _hitCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

            _shader.SetBuffer(0, "points", _pointBuffer);
        }

        public HitInformation[] FindUpwardSurfacePoints(ComputeBuffer triangleBuffer, int triangleCount,
            List<Vector3> points)
        {
            if (triangleCount == 0 || points.Count == 0)
            {
                return Array.Empty<HitInformation>();
            }

            _pointBuffer.SetData(points.ToArray());
            _hitBuffer.SetCounterValue(0);

            _shader.SetBuffer(0, "triangles", triangleBuffer);
            _shader.SetInt("num_triangles", triangleCount);
            _shader.SetInt("num_points", points.Count);
            _shader.SetBuffer(0, "hit_information_buffer", _hitBuffer);

            int numThreadsPerTriangles = Mathf.CeilToInt(triangleCount / 32f);
            int numThreadsPerPoints = Mathf.CeilToInt(points.Count / 32f);
            _shader.Dispatch(0, numThreadsPerTriangles, numThreadsPerPoints, 1);

            ComputeBuffer.CopyCount(_hitBuffer, _hitCountBuffer, 0);
            int[] hitCountArray = { 0 };
            _hitCountBuffer.GetData(hitCountArray);

            int numHits = hitCountArray[0];

            HitInformation[] hits = new HitInformation[numHits];
            _hitBuffer.GetData(hits);

            return hits;
        }
    }
}