using System;
using System.Collections.Generic;

namespace Items
{
    public class EquipmentComponent : EquipableComponent
    {
        // TODO: add enhancements, it is a placeholder for now, string should be replaced with an enum or something representing a player stat
        public EquipmentComponent(int slot, int durability, List<Tuple<string, int>> enhancements) : base(slot,
            durability)
        {
        }

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