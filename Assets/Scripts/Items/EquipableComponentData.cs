using System;
using Newtonsoft.Json;

namespace Items
{
    [Serializable]
    public abstract class EquipableComponentData : ItemComponentData
    {
        protected EquipableComponentData(int slot, int durability)
        {
            Slot = slot;
            Durability = durability;

            actions.Add(new ContextMenuAction("Equip", OnEquip));
            actions.Add(new ContextMenuAction("Unequip", OnUnequip));
        }

        // some examples, not yet sure what we need
        [JsonProperty("slot")] public int Slot { get; }

        [JsonProperty("durability")] public int Durability { get; }

        public abstract void OnEquip();
        public abstract void OnUnequip();
    }

    [Serializable]
    public abstract class EquipableComponent : ItemComponent
    {
        protected EquipableComponent(EquipableComponentData itemComponentData) : base(itemComponentData)
        {
            CurrentDurability = itemComponentData.Durability;
        }

        protected EquipableComponent(EquipableComponentData itemComponentData, int currentDurability) : base(
            itemComponentData)
        {
            CurrentDurability = currentDurability;
        }

        [JsonProperty("currentDurability")] public int CurrentDurability { get; protected set; }
    }
}