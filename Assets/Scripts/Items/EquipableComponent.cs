using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlayerControls;
using UI.Inventory.Components;

namespace Items
{
    [Serializable]
    public abstract class EquipableComponentData : ItemComponentData
    {
        protected EquipableComponentData(EquipmentSlotType slot, int durability)
        {
            Slot = slot;
            Durability = durability;

            actions.Add(new ContextMenuAction("Equip", (player, i) =>
            {
                if (OnEquip(player, i, null))
                {
                    player.playerInventory.RemoveItem(i.IDHash);
                    player.playerInventory.NotifySubscribersOnInventoryChanged();
                }
            }));
            actions.Add(new ContextMenuAction("Unequip", (player, i) =>
            {
                if (OnUnequip(player, i))
                {
                    player.playerInventory.AddItem(i);
                    player.playerInventory.NotifySubscribersOnInventoryChanged();
                }
            }));

            descriptors.Add(new KeyValuePair<string, string>("Slot", Slot.ToString()));
            descriptors.Add(new KeyValuePair<string, string>("Durability", Durability.ToString()));
        }

        // some examples, not yet sure what we need
        [JsonProperty("slot")] public EquipmentSlotType Slot { get; }

        [JsonProperty("durability")] public int Durability { get; }

        public abstract bool OnEquip(Player player, Item i, EquipmentSlot? slot);
        public abstract bool OnUnequip(Player player, Item i);
    }

    [Serializable]
    public abstract class EquipableComponent : ItemComponent
    {
        [JsonIgnore] public readonly EquipableComponentData equipableComponentData;

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