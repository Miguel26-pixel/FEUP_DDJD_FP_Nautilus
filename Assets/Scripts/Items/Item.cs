using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    public class ItemData : IInstantiable<Item>
    {
        [SerializeField] [JsonProperty] private string id;

        [SerializeField] [JsonProperty] private string name;

        [SerializeField] [JsonProperty] private string description;

        [SerializeField] [JsonProperty] protected string iconPath;

        // Only used for categorization of items in the inventory.
        [SerializeField] [JsonProperty] private ItemType type;

        // Used for adding different capabilities to items.
        [JsonProperty("components")] private List<ItemComponentData> _components = new();

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

            Sprite sprite = Resources.Load<Sprite>(iconPath);
            Icon = sprite;
            this.iconPath = iconPath;
        }

        public string ID => id;
        public int IDHash => int.Parse(id, NumberStyles.HexNumber);
        public string Name => name;
        public string Description => description;
        public ItemType Type => type;
        public Sprite Icon { get; }

        public Item CreateInstance()
        {
            return new Item(this);
        }

        public void AddComponent(ItemComponentData componentData)
        {
            _components.Add(componentData);
        }

        public IEnumerable<ItemComponentData> GetComponents()
        {
            return _components.ToArray();
        }
    }

    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Item
    {
        [JsonIgnore] private readonly ItemData _itemData;
        [JsonProperty("id")] private readonly string _itemID;

        [JsonProperty("components")] private List<ItemComponent> _components = new();

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
            _components = components;
        }

        public string ID => _itemID;
        public int IDHash => _itemData.IDHash;
        public string Name => _itemData.Name;
        public string Description => _itemData.Description;
        public ItemType Type => _itemData.Type;
        public Sprite Icon => _itemData.Icon;

        public void AddComponent(ItemComponent component)
        {
            _components.Add(component);
        }

        public T GetComponent<T>() where T : ItemComponent
        {
            return _components.OfType<T>().FirstOrDefault();
        }

        public T[] GetComponents<T>() where T : ItemComponent
        {
            return _components.OfType<T>().ToArray();
        }

        public IEnumerable<ItemComponent> GetComponents()
        {
            return _components.ToArray();
        }

        public bool HasComponent<T>() where T : ItemComponent
        {
            return _components.OfType<T>().Any();
        }

        public List<ContextMenuAction> GetContextMenuActions()
        {
            List<ContextMenuAction> actions = new();
            foreach (ItemComponent component in _components)
            {
                actions.AddRange(component.GetActions());
            }

            return actions;
        }
    }
}