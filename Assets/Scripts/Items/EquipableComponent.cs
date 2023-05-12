using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Items
{
    [Serializable]
    public abstract class EquipableComponent : ItemComponent
    {
        protected EquipableComponent(int slot, int durability)
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
}