using System;
using Newtonsoft.Json;

namespace Items
{
    [Serializable]
    public class WeaponComponent : EquipableComponent
    {
        public WeaponComponent(int slot, int durability, int damage, int range, int attackSpeed) : base(slot,
            durability)
        {
            Damage = damage;
            Range = range;
            AttackSpeed = attackSpeed;
        }

        // some examples, not yet sure what we need
        [JsonProperty("damage")] public int Damage { get; }

        [JsonProperty("range")] public int Range { get; }

        [JsonProperty("attackSpeed")] public int AttackSpeed { get; }

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