using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlayerControls;
using UnityEngine;
using UnityEngine.Serialization;

namespace Items
{
    [Serializable]
    public enum Enhancements
    {
        Speed = 0,
        SwimmingSpeed,
        OxygenCapacity,
        DiggingSpeed,
    }

    [Serializable]
    public class EquipmentComponentData : EquipableComponentData
    {
        [JsonIgnore]
        public Dictionary<Enhancements, string> enhancementNames = new()
        {
            { Items.Enhancements.Speed, "Speed" },
            { Items.Enhancements.SwimmingSpeed, "Swimming Speed" },
            { Items.Enhancements.DiggingSpeed, "Digging Speed" },
            { Items.Enhancements.OxygenCapacity, "Oxygen Capacity" },
        };

        [JsonProperty("enhancements")] private List<Tuple<Enhancements, int>> _enhancements;

        public EquipmentComponentData(int slot, int durability, List<Tuple<Enhancements, int>> enhancements) : base(slot,
            durability)
        {
            _enhancements = enhancements;

            foreach (Tuple<Enhancements, int> enhancement in enhancements)
            {
                descriptors.Add(new KeyValuePair<string, string>(enhancementNames[enhancement.Item1], enhancement.Item2.ToString()));
            }
        }

        [JsonIgnore] public List<Tuple<Enhancements, int>> Enhancements => new(_enhancements);

        private void EquipEnhancements(Player player)
        {
            foreach (var (enhancement, value) in _enhancements)
            {
                switch (enhancement)
                {
                    case Items.Enhancements.Speed:
                        player.playerController.AddSpeedBoost(value);
                        break;
                    case Items.Enhancements.SwimmingSpeed:
                        player.playerController.AddSwimmingBoost(value);
                        break;
                    default: 
                        Debug.Log("Not Implemented");
                        break;
                    // throw new NotImplementedException();
                }
            }
        }

        private void UnequipEnhancements(Player player)
        {
            foreach (var (enhancement, value) in _enhancements)
            {
                switch (enhancement)
                {
                    case Items.Enhancements.Speed:
                        player.playerController.AddSpeedBoost(-value);
                        break;
                    case Items.Enhancements.SwimmingSpeed:
                        player.playerController.AddSwimmingBoost(- value);
                        break;
                    default: 
                        Debug.Log("Not Implemented");
                        break;
                }
            }
        }
        
        public override void OnEquip(Player player, Item item)
        {
            EquipEnhancements(player);
            player.EquipEquipment(item);
        }

        public override void OnUnequip(Player player, Item item)
        {
            UnequipEnhancements(player);
            player.UnequipEquipment(item);
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