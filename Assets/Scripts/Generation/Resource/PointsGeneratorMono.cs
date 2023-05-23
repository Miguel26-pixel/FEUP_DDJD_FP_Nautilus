using System;
using System.Collections.Generic;
using UnityEngine;

namespace Generation.Resource
{
    public class PointsGeneratorMono : MonoBehaviour
    {
        // A list of settings for each resource type
        public List<ResourceGeneratorSettings> settings;
        public PointsGenerator pointsGenerator;

        public void Start()
        {
            MeshGenerator generator = GameObject.Find("GenerationManager").GetComponent<MeshGenerator>();
            pointsGenerator = new PointsGenerator(settings, generator.boundsSize, generator.seed);
        }
    }
}