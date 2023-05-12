using System;

namespace Items
{
    [Serializable]
    public class WeaponComponent : EquipableComponent
    {
        public WeaponComponent(int slot, int durability, string weapon) : base(slot,
            durability)
        {
            Weapon = weapon;
        }

        public string Weapon { get; }

        public override void OnEquip()
        {
            throw new NotImplementedException();
        }

        public override void OnUnequip()
        {
            throw new NotImplementedException();
        }
    }
}