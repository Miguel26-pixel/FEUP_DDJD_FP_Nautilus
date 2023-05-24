using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace Generation.Resource
{
    public record ResourceObject
    {
        public Vector3 position;
        public Quaternion rotation;
    }
    
    public class ResourceGenerator : MonoBehaviour
    {
        public LayerMask layerMask;
        // public GenerationConfigs generationConfigs;
        
        private RaycastSurfacePointsFinder _pointsFinder;
        private List<ResourceObject>[] _resourceObjects;
        private IObjectPool<GameObject>[] _objectPools;
        private ResourceGeneratorSettings[] _resourceGeneratorConfigs;

        private void Start()
        {
            MeshGenerator generator = GameObject.Find("GenerationManager").GetComponent<MeshGenerator>();
            _pointsFinder = new RaycastSurfacePointsFinder(layerMask, generator.boundsSize);
            _resourceGeneratorConfigs = generator.resourceGeneratorConfigs;
            _resourceObjects = new List<ResourceObject>[_resourceGeneratorConfigs.Length];
            _objectPools = new IObjectPool<GameObject>[_resourceGeneratorConfigs.Length];
            
            for (int i = 0; i < _resourceGeneratorConfigs.Length; i++)
            {
                _resourceObjects[i] = new List<ResourceObject>();
                // TODO: LATER ADD POOLING FOR OBJECTS, INSTATIATE THEM FOR NOW AS A TEST
                // _objectPools[i] = new ObjectPool<GameObject>(null, null, null, null, true);
            }
        }

        public void GenerateResources(Chunk chunk, float[,] biomeNoise , LinkedList<Vector2>[] points)
        {
            Random.State state = Random.state;
            Random.InitState(chunk.ChunkSeed());
            
            for (int i = 0; i < points.Length; i++)
            {
                ResourceGeneratorSettings settings = _resourceGeneratorConfigs[i];
                
                foreach (var point in points[i])
                {
                    HitInformation[] hits = _pointsFinder.FindUpwardSurfacePoints(chunk, point.x, point.y);

                    foreach (var hit in hits)
                    {
                        Vector3 surfacePoint = hit.position;
                        
                        if (settings.maxHeight < surfacePoint.y || settings.minHeight > surfacePoint.y)
                        {
                            continue;
                        }

                        Vector2Int biomeIndex = chunk.GetPointPosition(surfacePoint);
                        
                        float noise = biomeNoise[biomeIndex.y, biomeIndex.x];
                        if (noise < settings.minBiomeValue || noise > settings.maxBiomeValue)
                        {
                            continue;
                        }

                        if (settings.maxSlope < hit.slope || settings.minSlope > hit.slope)
                        {
                            continue;
                        }
                        
                        if (Random.value > settings.chanceOfGenerating)
                        {
                            continue;
                        }
                        
                        ResourceObject resourceObject = new ResourceObject()
                        {
                            position = surfacePoint,
                            rotation = Quaternion.Euler(0, Random.value * 360, 0)
                        };
                        _resourceObjects[i].Add(resourceObject);

                        // TODO: LATER ADD POOLING FOR OBJECTS, INSTATIATE THEM FOR NOW AS A TEST
                        Instantiate(settings.prefab, resourceObject.position, resourceObject.rotation);
                    }
                }
            }
            
            Random.state = state;
        }
    }
}