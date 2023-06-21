using System;
using Newtonsoft.Json;
using PlayerControls;
using UI.Inventory.Components;

namespace Items
{
    [Serializable]
    public class WeaponComponentData : EquipableComponentData
    {
        public WeaponComponentData(EquipmentSlotType slot, int durability, string weapon) : base(slot,
            durability)
        {
            Weapon = weapon;
        }

        [JsonProperty("weapon")] public string Weapon { get; }

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
            return new WeaponComponent(this);
        }
    }

    public class WeaponComponent : EquipableComponent
    {
        public WeaponComponent(WeaponComponentData itemComponentData) : base(itemComponentData)
        {
        }
    }
}