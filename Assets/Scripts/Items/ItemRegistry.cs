using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Items
{
    public class ItemRegistry : MonoBehaviour
    {
        private readonly Dictionary<int, Item> _items = new();
        public bool Initialized { get; private set; }

        /// <summary>
        ///     Creates a new item with the given name, description, and icon path and registers it in the registry.
        /// </summary>
        public Item CreateItem(string itemName, string description, string iconPath)
        {
            int hash = Hash(itemName, description);
            Item item = new(hash.ToString("X"), itemName, description, iconPath);
            _items.Add(hash, item);

            return item;
        }

        /// <summary>
        ///     Adds an item loaded from the JSON file to the registry.
        /// </summary>
        public void Add(Item item)
        {
            int key = int.Parse(item.ID, NumberStyles.HexNumber);
            _items.Add(key, item);
        }

        /// <summary>
        ///     Gets the item with the given string hash.
        ///     The use of a string hash is for better readability in the JSON file.
        /// </summary>
        /// <param name="hash">The string hash equivalent of the item's ID</param>
        public Item Get(string hash)
        {
            int key = int.Parse(hash, NumberStyles.HexNumber);
            return _items[key];
        }

        /// <summary>
        ///     Gets all items in the registry.
        /// </summary>
        /// <returns></returns>
        public Item[] GetAll()
        {
            return _items.Values.ToArray();
        }

        /// <summary>
        ///     Sets the registry as initialized.
        /// </summary>
        public void SetInitialized()
        {
            Initialized = true;
        }

        /// <summary>
        ///     Hashes the item's name and description to generate a unique ID.
        /// </summary>
        private int Hash(string itemName, string description)
        {
            string hashString = itemName + description;

            int hash = hashString.GetHashCode();
            while (_items.ContainsKey(hash)) hash = hash.GetHashCode();

            return hash;
        }
    }
}