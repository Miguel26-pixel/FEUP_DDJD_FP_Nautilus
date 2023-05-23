using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generation.Resource
{
    public class ResourceGeneratorMono : MonoBehaviour
    {
        // A list of settings for each resource type
        public List<ResourceGeneratorSettings> settings;
        public ResourceGenerator resourceGenerator;
        public int pointsCount = 0;
        public GameObject testObject;

        public void Start()
        {
            MeshGenerator generator = GameObject.Find("GenerationManager").GetComponent<MeshGenerator>();
            resourceGenerator = new ResourceGenerator(settings, generator.boundsSize, generator.seed);
        }

        public void Update()
        {
            if(resourceGenerator == null) return;
            
            if (resourceGenerator.pointsTest.Count > pointsCount)
            {
                pointsCount = resourceGenerator.pointsTest.Count;

                foreach (var point in resourceGenerator.pointsTest)
                {
                    Instantiate(testObject, new Vector3(point.x, 0, point.y), Quaternion.identity);
                }
                Debug.Log(pointsCount);
            }
        }
    }
}