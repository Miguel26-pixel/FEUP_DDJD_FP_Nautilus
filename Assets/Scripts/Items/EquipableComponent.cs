using System;
using System.Collections.Generic;
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

            descriptors.Add(new KeyValuePair<string, string>("Slot", Slot.ToString()));
            descriptors.Add(new KeyValuePair<string, string>("Durability", Durability.ToString()));
        }

        // some examples, not yet sure what we need
        [JsonProperty("slot")] public int Slot { get; }

        [JsonProperty("durability")] public int Durability { get; }

        public abstract void OnEquip(Player.Player player);
        public abstract void OnUnequip(Player.Player player);
    }

    [Serializable]
    public abstract class EquipableComponent : ItemComponent
    {
        [JsonIgnore] protected readonly EquipableComponentData equipableComponentData;

        [JsonProperty("currentDurability")] private int _currentDurability;

        protected EquipableComponent(EquipableComponentData itemComponentData) : base(itemComponentData)
        {
            equipableComponentData = itemComponentData;
            CurrentDurability = itemComponentData.Durability;
        }

        [JsonIgnore]
        public int CurrentDurability
        {
            get => _currentDurability;
            set
            {
                if (value < 0)
                {
                    _currentDurability = 0;
                }
                else if (value > equipableComponentData.Durability)
                {
                    _currentDurability = equipableComponentData.Durability;
                }
                else
                {
                    _currentDurability = value;
                }
            }
        }
    }
}