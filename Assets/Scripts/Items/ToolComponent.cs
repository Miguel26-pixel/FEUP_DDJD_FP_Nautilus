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

        public override void OnEquip(Player.Player player)
        {
            throw new NotImplementedException();
        }

        public override void OnUnequip(Player.Player player)
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
        public ToolComponent(ToolComponentData itemComponentData) : base(itemComponentData)
        {
        }
    }
}