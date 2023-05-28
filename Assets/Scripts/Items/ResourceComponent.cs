using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Items
{
    [Serializable]
    public class ResourceComponentData : ItemComponentData
    {
        public ResourceComponentData(int neededCollectionCount)
        {
            NeededCollectionCount = neededCollectionCount;
        }

        // This is the number of times the resource has to be collected before it is added to the inventory.
        [JsonProperty("neededCollectionCount")] public int NeededCollectionCount { get;}
        
        public override ItemComponent CreateInstance()
        {
            return new ResourceComponent(this);
        }
    }

    [Serializable]
    public class ResourceComponent : ItemComponent
    {
        public ResourceComponent(ResourceComponentData itemComponentData) : base(itemComponentData)
        {
        }
        
        public int NeededCollectionCount => ((ResourceComponentData) itemComponentData).NeededCollectionCount;
    }
}