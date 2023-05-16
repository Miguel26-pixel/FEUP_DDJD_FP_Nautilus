using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Items
{
    [Serializable]
    public class EquipmentComponentData : EquipableComponentData
    {
        [JsonProperty] private List<Tuple<string, int>> _enhancements;

        // TODO: add enhancements, it is a placeholder for now, string should be replaced with an enum or something representing a player stat
        public EquipmentComponentData(int slot, int durability, List<Tuple<string, int>> enhancements) : base(slot,
            durability)
        {
            _enhancements = enhancements;
        }

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
            return new EquipmentComponent(this);
        }
    }

    public class EquipmentComponent : EquipableComponent
    {
        public EquipmentComponent(EquipableComponentData itemComponentData) : base(itemComponentData)
        {
        }

        public EquipmentComponent(EquipableComponentData itemComponentData, int currentDurability) : base(
            itemComponentData, currentDurability)
        {
        }
    }
}