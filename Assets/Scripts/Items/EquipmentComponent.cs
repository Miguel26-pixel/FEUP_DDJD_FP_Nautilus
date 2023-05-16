using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Items
{
    [Serializable]
    public class EquipmentComponentData : EquipableComponentData
    {
        [JsonProperty("enhancements")] private List<Tuple<string, int>> _enhancements;

        // TODO: add enhancements, it is a placeholder for now, string should be replaced with an enum or something representing a player stat
        public EquipmentComponentData(int slot, int durability, List<Tuple<string, int>> enhancements) : base(slot,
            durability)
        {
            _enhancements = enhancements;
        }

        [JsonIgnore] public List<Tuple<string, int>> Enhancements => new(_enhancements);

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
        public EquipmentComponent(EquipmentComponentData itemComponentData) : base(itemComponentData)
        {
        }
    }
}