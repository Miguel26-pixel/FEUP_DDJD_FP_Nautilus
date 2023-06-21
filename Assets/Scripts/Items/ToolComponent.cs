using System;
using Newtonsoft.Json;
using PlayerControls;
using UI.Inventory.Components;

namespace Items
{
    [Serializable]
    public class ToolComponentData : EquipableComponentData
    {
        public ToolComponentData(EquipmentSlotType slot, int durability, string tool) : base(slot, durability)
        {
            Tool = tool;
        }

        [JsonProperty("tool")] public string Tool { get; }

        public override bool OnEquip(Player player, Item item, EquipmentSlot? slot)
        {
            throw new NotImplementedException();
        }

        public override bool OnUnequip(Player player, Item item)
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