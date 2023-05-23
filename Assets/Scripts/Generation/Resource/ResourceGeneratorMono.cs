using System.Collections.Generic;
using UnityEngine;

namespace Generation.Resource
{
    public class ResourceGeneratorMono : MonoBehaviour
    {
        // A list of settings for each resource type
        public List<ResourceGeneratorSettings> settings;
        public ResourceGenerator resourceGenerator;

        public void Start()
        {
            MeshGenerator generator = GameObject.Find("GenerationManager").GetComponent<MeshGenerator>();
            resourceGenerator = new ResourceGenerator(settings, generator.boundsSize, generator.seed);
        }
    }
}