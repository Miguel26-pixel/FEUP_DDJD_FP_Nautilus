using System;
using UnityEngine;

namespace Items
{
    [Serializable]
    public class Item
    {
        [SerializeField]
        private string id;
        [SerializeField]
        private string name;
        [SerializeField]
        private string description;
        [SerializeField]
        private string iconPath;


        public string ID => id;
        public string Name => name;
        public string Description => description;
        public Sprite Icon { get; }


        /// <summary>
        /// Creates a new item with the given name, description, and icon.
        /// </summary>
        public Item(string name, string description, string icon)
        {
            this.name = name;
            this.description = description;
        
            Sprite sprite = Resources.Load<Sprite>(icon); 
            Icon = sprite;
            iconPath = icon;
        
            // Hash name and description to generate an ID
            id = ItemRegistry.Add(this);
        }
        
        /// <summary>
        /// Creates a new item from JSON data.
        /// </summary>
        public Item(string id, string name, string description, string icon)
        {
            this.id = id;
            this.name = name;
            this.description = description;
        
            Sprite sprite = Resources.Load<Sprite>(icon); 
            Icon = sprite;
            iconPath = icon;
            
            // Add item to registry
            ItemRegistry.Add(id, this);
        }
    }
}
