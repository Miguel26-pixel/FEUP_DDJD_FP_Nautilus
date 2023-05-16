using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Items
{
    public static class ItemConstants
    {
        public const int ItemWidth = 4;
        public const int ItemHeight = 4;
    }

    [Serializable]
    public enum ItemType
    {
        Resource,
        Fruit,
        CreatureDrop,
        Consumable,
        Weapon,
        Equipment,
        Machine,
        Metal,
        Tool
    }

    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class ItemData : ItemEntity<ItemComponentData>, IInstantiable<Item>
    {
        [SerializeField] [JsonProperty] private string id;

        [SerializeField] [JsonProperty] private string name;

        [SerializeField] [JsonProperty] private string description;

        [SerializeField] [JsonProperty] protected string iconPath;

        // Only used for categorization of items in the inventory.
        [SerializeField] [JsonProperty] private ItemType type;

        /// <summary>
        ///     Creates a new item with the given id, name, description, and icon.
        /// </summary>
        [JsonConstructor]
        public ItemData(string id, string name, string description, ItemType type, string iconPath)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.type = type;
            this.iconPath = iconPath;

            Icons = new Sprite[ItemConstants.ItemHeight, ItemConstants.ItemWidth];
            Sprite[] spriteSheet = Resources.LoadAll<Sprite>(iconPath);
            
            foreach (Sprite sprite in spriteSheet)
            {
                string spriteName = sprite.name;
                string[] split = spriteName.Split('_');
                int y = int.Parse(split[1]);
                int x = int.Parse(split[2]);
                Icons[y, x] = sprite;
            }
        }

        public string ID => id;
        public int IDHash => int.Parse(id, NumberStyles.HexNumber);
        public string Name => name;
        public string Description => description;
        public ItemType Type => type;
        public Sprite[,] Icons { get; }

        public Item CreateInstance()
        {
            return new Item(this);
        }
    }

    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Item : ItemEntity<ItemComponent>
    {
        [JsonIgnore] private readonly ItemData _itemData;
        [JsonProperty("id")] private readonly string _itemID;

        public Item(ItemData itemData)
        {
            _itemData = itemData;
            _itemID = itemData.ID;

            foreach (ItemComponentData component in itemData.GetComponents())
            {
                AddComponent(component.CreateInstance());
            }
        }

        public Item(ItemData itemData, List<ItemComponent> components)
        {
            _itemData = itemData;
            _itemID = itemData.ID;
            this.components = components;
        }

        public string ID => _itemID;
        public int IDHash => _itemData.IDHash;
        public string Name => _itemData.Name;
        public string Description => _itemData.Description;
        public ItemType Type => _itemData.Type;
        public Sprite[,] Icons => _itemData.Icons;

        public List<ContextMenuAction> GetContextMenuActions()
        {
            List<ContextMenuAction> actions = new();
            foreach (ItemComponent component in components)
            {
                actions.AddRange(component.GetActions());
            }

            return actions;
        }
    }
}