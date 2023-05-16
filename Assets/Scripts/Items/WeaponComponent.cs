using System;
using Newtonsoft.Json;

namespace Items
{
    [Serializable]
    public class WeaponComponentData : EquipableComponentData
    {
        public WeaponComponentData(int slot, int durability, string weapon) : base(slot,
            durability)
        {
            Weapon = weapon;
        }

        [JsonProperty("weapon")] public string Weapon { get; }

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
            return new WeaponComponent(this);
        }
    }

    public class WeaponComponent : EquipableComponent
    {
        public WeaponComponent(EquipableComponentData itemComponentData) : base(itemComponentData)
        {
        }

        public WeaponComponent(EquipableComponentData itemComponentData, int currentDurability) : base(
            itemComponentData, currentDurability)
        {
        }
    }
}