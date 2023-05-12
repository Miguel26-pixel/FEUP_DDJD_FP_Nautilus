using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Items
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Item
    {
        [SerializeField]
        [JsonProperty]
        private string id;
        [SerializeField]
        [JsonProperty]
        private string name;
        [SerializeField]
        [JsonProperty]
        private string description;
        [SerializeField]
        [JsonProperty]
        private string iconPath;
        
        public string ID => id;
        public string Name => name;
        public string Description => description;
        public Sprite Icon { get; }


        /// <summary>
        /// Creates a new item with the given id, name, description, and icon.
        /// </summary>
        [JsonConstructor]
        public Item(string id, string name, string description, string icon)
        {
            this.id = id;
            this.name = name;
            this.description = description;
        
            Sprite sprite = Resources.Load<Sprite>(icon); 
            Icon = sprite;
            iconPath = icon;
        }
    }
}
