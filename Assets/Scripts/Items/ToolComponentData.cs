using System;
using Newtonsoft.Json;

namespace Items
{
    [Serializable]
    public class ToolComponentData : EquipableComponentData
    {
        public ToolComponentData(int slot, int durability, string tool) : base(slot, durability)
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

        public override ItemComponent CreateInstance()
        {
            return new ToolComponent(this);
        }
    }

    public class ToolComponent : EquipableComponent
    {
        public ToolComponent(EquipableComponentData itemComponentData) : base(itemComponentData)
        {
        }

        public ToolComponent(EquipableComponentData itemComponentData, int currentDurability) : base(itemComponentData,
            currentDurability)
        {
        }
    }
}