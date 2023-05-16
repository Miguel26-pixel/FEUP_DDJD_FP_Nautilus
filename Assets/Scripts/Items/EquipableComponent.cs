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
        [JsonProperty("currentDurability")]
        private int _currentDurability;
        [JsonIgnore]
        protected readonly EquipableComponentData equipableComponentData;
        
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
                } else if (value > equipableComponentData.Durability)
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