using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Items
{
    public enum ItemType
    {
        Resource,
        Fruit,
        CreatureDrop,
        Consumable,
        Weapon,
        Equipment,
        Machine,
    }

    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Item
    {
        [SerializeField] [JsonProperty] private string id;

        [SerializeField] [JsonProperty] private string name;

        [SerializeField] [JsonProperty] private string description;

        [SerializeField] [JsonProperty] private string iconPath;

        // Only used for categorization of items in the inventory.
        [SerializeField] [JsonProperty] private ItemType type;

        // Used for adding different capabilities to items.
        [SerializeField] [JsonProperty] private List<ItemComponent> components = new();


        /// <summary>
        ///     Creates a new item with the given id, name, description, and icon.
        /// </summary>
        [JsonConstructor]
        public Item(string id, string name, string description, ItemType type, string icon)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.type = type;

            Sprite sprite = Resources.Load<Sprite>(icon);
            Icon = sprite;
            iconPath = icon;
        }

        public string ID => id;
        public string Name => name;
        public string Description => description;
        public ItemType Type => type;
        public Sprite Icon { get; }

        public void AddComponent(ItemComponent component)
        {
            components.Add(component);
        }

        public T GetComponent<T>() where T : ItemComponent
        {
            return components.OfType<T>().FirstOrDefault();
        }

        public T[] GetComponents<T>() where T : ItemComponent
        {
            return components.OfType<T>().ToArray();
        }

        public bool HasComponent<T>() where T : ItemComponent
        {
            return components.OfType<T>().Any();
        }
        
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