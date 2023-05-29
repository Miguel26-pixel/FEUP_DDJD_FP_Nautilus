using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace Generation.Resource
{
    public record ResourceData
    {
        public Vector3 position;
        public Quaternion rotation;
        public int activeIndex;
    }

    public record ActiveObject
    {
        public ResourceData resourceData;
        public GameObject gameObject;
    }

    public class ResourceGenerator : MonoBehaviour, IDisposable
    {
        public LayerMask layerMask;
        public GameObject resourceParent;
        public ComputeShader pointsFinderShader;

        private BarycentricSurfacePointsFinder _barycentricSurfacePointsFinder;
        private Dictionary<Vector3Int, List<ResourceData>[]> _resourceObjects;
        private Dictionary<Vector3Int, List<ActiveObject>[]> _activeResourceObjects;
        private IObjectPool<GameObject>[] _objectPools;
        private ResourceGeneratorSettings[] _resourceGeneratorConfigs;
        private HashSet<Vector3Int> _previouslyActiveChunks = new();
        private int _numPointsPerAxis;

        private void Start()
        {
            MeshGenerator generator = GameObject.Find("GenerationManager").GetComponent<MeshGenerator>();
            PointsGeneratorMono pointsGeneratorMono = GetComponent<PointsGeneratorMono>();
            _resourceGeneratorConfigs = generator.resourceGeneratorConfigs;
            _resourceObjects = new Dictionary<Vector3Int, List<ResourceData>[]>();
            _objectPools = new IObjectPool<GameObject>[_resourceGeneratorConfigs.Length];
            _activeResourceObjects = new Dictionary<Vector3Int, List<ActiveObject>[]>();
            _numPointsPerAxis = generator.numPointsPerAxis;
            
            int numVoxelsPerAxis = _numPointsPerAxis - 1;
            int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
            int maxTriangleCount = numVoxels * 5;

            _barycentricSurfacePointsFinder = new BarycentricSurfacePointsFinder(pointsFinderShader,
                pointsGeneratorMono.pointsGenerator.MaxPoints(), maxTriangleCount);


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

        public void CheckGrounded(Vector3Int chunkPosition)
        {
            List<ActiveObject>[] activeObjects = _activeResourceObjects[chunkPosition];
            List<ResourceData>[] resourceObjects = _resourceObjects[chunkPosition];

            for (int i = 0; i < activeObjects.Length; i++)
            {
                float offset = _resourceGeneratorConfigs[i].offset;
                IObjectPool<GameObject> objectPool = _objectPools[i];
                List<ActiveObject> activeObjectList = new List<ActiveObject>(activeObjects[i]);
                List<ResourceData> resourceDataList = resourceObjects[i];

                foreach (var resource in activeObjectList)
                {
                    var resourceObject = resource.gameObject;
                    if (resourceObject == null)
                    {
                        _activeResourceObjects[chunkPosition][i].Remove(resource);
                        continue;
                    }
                    if (resourceObject.activeSelf)
                    {
                        if (!Physics.BoxCast(resourceObject.transform.position + resourceObject.transform.up.normalized * (-offset + 0.5f) , new Vector3(0.1f,0.4f,0.1f), -resourceObject.transform.up.normalized, out _, resourceObject.transform.rotation,  -offset + 0.2f, layerMask))
                        {
                            GameObject droppedObject = Instantiate(resourceObject, resourceObject.transform.position, resourceObject.transform.rotation);
                            
                            objectPool.Release(resourceObject.gameObject);
                            _activeResourceObjects[chunkPosition][i].Remove(resource);
                            resourceDataList.Remove(resource.resourceData);

                            droppedObject.transform.GetChild(resource.resourceData.activeIndex).gameObject
                                .AddComponent<Rigidbody>();
                            droppedObject.GetComponent<Resource>().dropped = true;
                            
                            DestroySelf destroySelf = droppedObject.AddComponent<DestroySelf>();
                            destroySelf.SetTimer(30f);
                        }
                    }
                }
            }
        }

        public void GenerateResources(Chunk chunk, float[] biomeNoise , LinkedList<Vector2>[] points)
        {
            Random.State state = Random.state;
            Random.InitState(chunk.ChunkSeed());
            _resourceObjects[chunk.chunkGridPosition] = new List<ResourceData>[_resourceGeneratorConfigs.Length];
            
            List<Vector3> pointsWithIndex = new List<Vector3>();
            List<ResourceData>[] resourceObjects = _resourceObjects[chunk.chunkGridPosition];

            for (int i = 0; i < points.Length; i++)
            {
                foreach (var point in points[i])
                {
                    pointsWithIndex.Add(new Vector3(point.x, point.y, i));
                }
            }

            Tuple<ComputeBuffer, int> triangles = chunk.GetTriangleBuffer();

            HitInformation[] hits = _barycentricSurfacePointsFinder.FindUpwardSurfacePoints(triangles.Item1, triangles.Item2, pointsWithIndex);

            for (int i = 0; i < points.Length; i++)
            {
                resourceObjects[i] = new List<ResourceData>();
                _resourceObjects[chunk.chunkGridPosition][i] = resourceObjects[i];
            }
           
            foreach (var hit in hits)
            {
                int i = Mathf.RoundToInt(hit.position.w);
                ResourceGeneratorSettings settings = _resourceGeneratorConfigs[i];
                
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

                float slope = Vector3.Dot(hit.normal, Vector3.up);

                if (settings.maxSlope < slope || settings.minSlope > slope)
                {
                    continue;
                }
                        
                if (Random.value > settings.chanceOfGenerating)
                {
                    continue;
                }
                        
                int activeIndex = Random.Range(0, settings.prefab.transform.childCount);
                Vector3 point = surfacePoint + hit.normal * settings.offset;
                        
                ResourceData resourceData = new ResourceData()
                {
                    position = point,
                    rotation = 
                        (settings.alignToSurface ? Quaternion.FromToRotation(Vector3.up, hit.normal) : Quaternion.identity) *
                        Quaternion.Euler(0, Random.value * 360, 0),
                    activeIndex = activeIndex
                };
                resourceObjects[i].Add(resourceData);
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
            _activeResourceObjects[chunkGridPosition] = new List<ActiveObject>[_resourceGeneratorConfigs.Length];

            for (int i = 0; i < _resourceGeneratorConfigs.Length; i++)
            {
                List<ResourceData> resourceObjects = _resourceObjects[chunkGridPosition][i];
                IObjectPool<GameObject> objectPool = _objectPools[i];
                _activeResourceObjects[chunkGridPosition][i] = new List<ActiveObject>();

                foreach (var resourceObject in resourceObjects)
                {
                    GameObject gameObject = objectPool.Get();
                    gameObject.transform.position = resourceObject.position;
                    gameObject.transform.rotation = resourceObject.rotation;
                    gameObject.transform.GetChild(resourceObject.activeIndex).gameObject.SetActive(true);
                    
                    _activeResourceObjects[chunkGridPosition][i].Add(new ActiveObject { gameObject = gameObject, resourceData = resourceObject});
                }
            }
        }

        private void RemoveResources(Vector3Int chunkGridPosition)
        {
            for (int i = 0; i < _resourceGeneratorConfigs.Length; i++)
            {
                if (!_activeResourceObjects.TryGetValue(chunkGridPosition, out List<ActiveObject>[] resourceObjectsList))
                {
                    continue;
                }
                
                List<ActiveObject> resourceObjects = new List<ActiveObject>(resourceObjectsList[i]);
                IObjectPool<GameObject> objectPool = _objectPools[i];
                
                foreach (var resourceObject in resourceObjects)
                {
                    objectPool.Release(resourceObject.gameObject);
                    _activeResourceObjects[chunkGridPosition][i].Remove(resourceObject);
                }
            }
        }

        public void Dispose()
        {
            _barycentricSurfacePointsFinder?.Dispose();
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}