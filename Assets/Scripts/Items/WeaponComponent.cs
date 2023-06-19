using System;
using Newtonsoft.Json;
using PlayerControls;

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

        public override void OnEquip(Player player)
        {
            throw new NotImplementedException();
        }

        public override void OnUnequip(Player player)
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