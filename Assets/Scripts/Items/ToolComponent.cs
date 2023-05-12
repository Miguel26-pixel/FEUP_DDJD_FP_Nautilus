using System;
using Newtonsoft.Json;

namespace Items
{
    [Serializable]
    public class ToolComponent : EquipableComponent
    {
        public ToolComponent(int slot, int durability, string tool) : base(slot, durability)
        {
            Tool = tool;
        }

        [JsonProperty("tool")] public string Tool { get; }

        public override void OnEquip()
        {
            throw new NotImplementedException();
        }

        public override void OnUnequip()
        {
            throw new NotImplementedException();
        }
    }
}