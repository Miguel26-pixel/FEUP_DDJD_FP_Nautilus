using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Items
{
    [Serializable]
    public class EquipmentComponent : EquipableComponent
    {
        [JsonProperty] private List<Tuple<string, int>> _enhancements;
        
        // TODO: add enhancements, it is a placeholder for now, string should be replaced with an enum or something representing a player stat
        public EquipmentComponent(int slot, int durability, List<Tuple<string, int>> enhancements) : base(slot,
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
    }
}