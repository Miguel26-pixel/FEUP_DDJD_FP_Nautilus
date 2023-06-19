using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generation.Resource
{
    public record HitInformation
    {
        public Vector3 position;
        public Vector3 normal;
        public float slope;
    }
    
    public class RaycastSurfacePointsFinder
    {
        private readonly float _boundsSize;
        private readonly LayerMask _layerMask;

        public RaycastSurfacePointsFinder(LayerMask layerMask, float boundsSize)
        {
            _layerMask = layerMask;
            _boundsSize = boundsSize;
        }

        public HitInformation[] FindUpwardSurfacePoints(Chunk chunk, float x, float z)
        {
            Vector3 yPos = new Vector3(0, chunk.chunkGridPosition.y * _boundsSize + _boundsSize / 2 + 5, 0);
            yPos = chunk.transform.TransformPoint(yPos);
            
            Vector3 rayStart = new Vector3(x, 
                yPos.y, 
                z);

            RaycastHit[] results = new RaycastHit[5];
            var size = Physics.RaycastNonAlloc(rayStart, Vector3.down, results, _boundsSize + 5, _layerMask);

            List<Vector3> vectorResults = new List<Vector3>();
            for (int i = 0; i < size; i++)
            {
                if (Vector3.Dot(results[i].normal, Vector3.up) > 0)
                {
                    vectorResults.Add(results[i].point);
                }
            }
            
            HitInformation[] hitInformations = new HitInformation[vectorResults.Count];
            for (int i = 0; i < vectorResults.Count; i++)
            {
                hitInformations[i] = new HitInformation()
                {
                    position = vectorResults[i],
                    normal = results[i].normal,
                    slope = Vector3.Dot(results[i].normal, Vector3.up)
                };
            }
            
            return hitInformations;
        }
    }
}