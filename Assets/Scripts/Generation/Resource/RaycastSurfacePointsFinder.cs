using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generation.Resource
{
    public class RaycastSurfacePointsFinder
    {
        private readonly Chunk _chunk;
        private readonly float _boundsSize;
        private readonly LayerMask _layerMask;

        public RaycastSurfacePointsFinder(Chunk chunk, LayerMask layerMask, float boundsSize)
        {
            _chunk = chunk;
            _layerMask = layerMask;
            _boundsSize = boundsSize;
        }

        public Vector3[] FindUpwardSurfacePoints(float x, float z)
        {
            Vector3 yPos = new Vector3(0, _chunk.chunkGridPosition.y * _boundsSize + _boundsSize / 2 + 5, 0);
            yPos = _chunk.transform.TransformPoint(yPos);
            
            Vector3 rayStart = new Vector3(x, 
                yPos.y, 
                z);

            RaycastHit[] results = new RaycastHit[5];
            var size = Physics.RaycastNonAlloc(rayStart, Vector3.down, results, _boundsSize * 1.2f, _layerMask);

            List<Vector3> vectorResults = new List<Vector3>();
            for (int i = 0; i < size; i++)
            {
                if (Vector3.Dot(results[i].normal, Vector3.up) > 0)
                {
                    vectorResults.Add(results[i].point);
                }
            }

            return vectorResults.ToArray();
        }
    }
}