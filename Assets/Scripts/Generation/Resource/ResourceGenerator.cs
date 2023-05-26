using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace Generation.Resource
{
    public record ResourceObject
    {
        public Vector3 position;
        public Quaternion rotation;
        public int activeIndex;
    }
    
    public class ResourceGenerator : MonoBehaviour
    {
        public LayerMask layerMask;
        public GameObject resourceParent;

        private RaycastSurfacePointsFinder _pointsFinder;
        private Dictionary<Vector3Int, List<ResourceObject>[]> _resourceObjects;
        private Dictionary<Vector3Int, List<GameObject>[]> _activeResourceObjects;
        private IObjectPool<GameObject>[] _objectPools;
        private ResourceGeneratorSettings[] _resourceGeneratorConfigs;
        private HashSet<Vector3Int> _previouslyActiveChunks = new();
        private int _numPointsPerAxis;

        private void Start()
        {
            MeshGenerator generator = GameObject.Find("GenerationManager").GetComponent<MeshGenerator>();
            _pointsFinder = new RaycastSurfacePointsFinder(layerMask, generator.boundsSize);
            _resourceGeneratorConfigs = generator.resourceGeneratorConfigs;
            _resourceObjects = new Dictionary<Vector3Int, List<ResourceObject>[]>();
            _objectPools = new IObjectPool<GameObject>[_resourceGeneratorConfigs.Length];
            _activeResourceObjects = new Dictionary<Vector3Int, List<GameObject>[]>();
            _numPointsPerAxis = generator.numPointsPerAxis;

            for (int i = 0; i < _resourceGeneratorConfigs.Length; i++)
            {
                int i1 = i;
                _objectPools[i] = new ObjectPool<GameObject>(() =>
                {
                    GameObject gameObject = Instantiate(_resourceGeneratorConfigs[i1].prefab, resourceParent.transform, true);
                    gameObject.SetActive(false);
                    return gameObject;
                }, OnTakeFromPool, defaultCapacity: 500, actionOnRelease: OnReturnToPool, actionOnDestroy: OnDestroyFromPool);
            }
        }
        
        private static void OnTakeFromPool(GameObject gameObject)
        {
            gameObject.SetActive(true);
            int childCount = gameObject.transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                gameObject.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        
        private static void OnReturnToPool(GameObject gameObject)
        {
            gameObject.SetActive(false);
        }
        
        private static void OnDestroyFromPool(GameObject gameObject)
        {
            Destroy(gameObject);
        }

        public void GenerateResources(Chunk chunk, float[] biomeNoise , LinkedList<Vector2>[] points)
        {
            Random.State state = Random.state;
            Random.InitState(chunk.ChunkSeed());
            _resourceObjects[chunk.chunkGridPosition] = new List<ResourceObject>[_resourceGeneratorConfigs.Length];
            
            for (int i = 0; i < points.Length; i++)
            {
                ResourceGeneratorSettings settings = _resourceGeneratorConfigs[i];
                _resourceObjects[chunk.chunkGridPosition][i] = new List<ResourceObject>();
                List<ResourceObject> resourceObjects = _resourceObjects[chunk.chunkGridPosition][i];
                
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
                        
                        float noise = biomeNoise[biomeIndex.y * _numPointsPerAxis + biomeIndex.x];
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
                        
                        int activeIndex = Random.Range(0, settings.prefab.transform.childCount);
                        
                        ResourceObject resourceObject = new ResourceObject()
                        {
                            position = surfacePoint,
                            rotation = 
                                (settings.alignToSurface ? Quaternion.FromToRotation(Vector3.up, hit.normal) : Quaternion.identity) *
                                Quaternion.Euler(0, Random.value * 360, 0),
                            activeIndex = activeIndex
                        };
                        resourceObjects.Add(resourceObject);
                    }
                }
            }
            
            Random.state = state;
        }

        public void UpdateResources(HashSet<Vector3Int> activeChunks)
        {
            foreach (var chunk in _previouslyActiveChunks)
            {
                if (!activeChunks.Contains(chunk))
                {
                    RemoveResources(chunk);
                }
            }
            
            foreach (var chunk in activeChunks)
            {
                if (!_previouslyActiveChunks.Contains(chunk))
                {
                    GetResources(chunk);
                }
            }

            _previouslyActiveChunks = new HashSet<Vector3Int>(activeChunks);
        }

        private void GetResources(Vector3Int chunkGridPosition)
        {
            _activeResourceObjects[chunkGridPosition] = new List<GameObject>[_resourceGeneratorConfigs.Length];

            for (int i = 0; i < _resourceGeneratorConfigs.Length; i++)
            {
                List<ResourceObject> resourceObjects = _resourceObjects[chunkGridPosition][i];
                IObjectPool<GameObject> objectPool = _objectPools[i];
                _activeResourceObjects[chunkGridPosition][i] = new List<GameObject>();

                foreach (var resourceObject in resourceObjects)
                {
                    GameObject gameObject = objectPool.Get();
                    gameObject.transform.position = resourceObject.position;
                    gameObject.transform.rotation = resourceObject.rotation;
                    gameObject.transform.GetChild(resourceObject.activeIndex).gameObject.SetActive(true);
                    
                    _activeResourceObjects[chunkGridPosition][i].Add(gameObject);
                }
            }
        }

        private void RemoveResources(Vector3Int chunkGridPosition)
        {
            for (int i = 0; i < _resourceGeneratorConfigs.Length; i++)
            {
                if (!_activeResourceObjects.TryGetValue(chunkGridPosition, out List<GameObject>[] resourceObjectsList))
                {
                    continue;
                }
                
                List<GameObject> resourceObjects = new List<GameObject>(resourceObjectsList[i]);
                IObjectPool<GameObject> objectPool = _objectPools[i];
                
                foreach (var resourceObject in resourceObjects)
                {
                    objectPool.Release(resourceObject);
                    _activeResourceObjects[chunkGridPosition][i].Remove(resourceObject);
                }
            }
        }
    }
}