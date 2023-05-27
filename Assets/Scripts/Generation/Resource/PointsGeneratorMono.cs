using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Generation.Resource
{
    public class PointsGeneratorMono : MonoBehaviour
    {
        public PointsGenerator pointsGenerator;

        public void Awake()
        {
            MeshGenerator generator = GameObject.Find("GenerationManager").GetComponent<MeshGenerator>();
            pointsGenerator = new PointsGenerator(generator.resourceGeneratorConfigs , generator.boundsSize, generator.seed);
        }
    }
}