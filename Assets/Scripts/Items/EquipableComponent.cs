using System;
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
        }

        // some examples, not yet sure what we need
        [JsonProperty("slot")] public int Slot { get; }

        [JsonProperty("durability")] public int Durability { get; }

        public void Equip()
        {
            throw new NotImplementedException();
            // do stuff, then call
            // OnEquip();
        }

        public void Unequip()
        {
            throw new NotImplementedException();
            // do stuff, then call
            // OnUnequip();
        }

        public abstract void OnEquip();
        public abstract void OnUnequip();
    }
}