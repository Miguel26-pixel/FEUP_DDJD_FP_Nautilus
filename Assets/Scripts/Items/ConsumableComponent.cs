using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Items
{
    [Serializable]
    public class ConsumableComponentData : ItemComponentData
    {
        public ConsumableComponentData(int health, int hunger)
        {
            Health = health;
            Hunger = hunger;

            actions.Add(new ContextMenuAction("Consume", Consume));

            descriptors.Add(new KeyValuePair<string, string>("Health", Health.ToString()));
            descriptors.Add(new KeyValuePair<string, string>("Hunger", Hunger.ToString()));
        }

        [JsonProperty("health")] public int Health { get; }

        [JsonProperty("hunger")] public int Hunger { get; }

        private void Consume()
        {
            throw new NotImplementedException();
        }

        public override ItemComponent CreateInstance()
        {
            return new ConsumableComponent(this);
        }
    }

    [Serializable]
    public class ConsumableComponent : ItemComponent
    {
        public ConsumableComponent(ConsumableComponentData itemComponentData) : base(itemComponentData)
        {
        }
    }
}