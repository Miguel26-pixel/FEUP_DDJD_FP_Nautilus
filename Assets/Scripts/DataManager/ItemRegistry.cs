using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Items;

namespace DataManager
{
    public class ItemRegistry : Registry<Item>
    {
        private readonly Dictionary<int, Item> _items = new();

        /// <summary>
        ///     Creates a new item with the given name, description, and icon path and registers it in the registry.
        /// </summary>
        public Item CreateItem(string itemName, string description, ItemType type, string iconPath)
        {
            int hash = Hash(itemName, description);
            Item item = new(hash.ToString("X"), itemName, description, type, iconPath);
            _items.Add(hash, item);

            return item;
        }

        /// <summary>
        ///     Adds an item loaded from the JSON file to the registry.
        /// </summary>
        public override void Add(Item item)
        {
            _items.Add(item.IDHash, item);
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

        public Item Get(int hash)
        {
            return _items[hash];
        }

        /// <summary>
        ///     Gets all items in the registry.
        /// </summary>
        /// <returns></returns>
        public override Item[] GetAll()
        {
            return _items.Values.ToArray();
        }

        /// <summary>
        ///     Hashes the item's name and description to generate a unique ID.
        /// </summary>
        private int Hash(string itemName, string description)
        {
            string hashString = itemName + description;

            int hash = hashString.GetHashCode();
            while (_items.ContainsKey(hash))
            {
                hash = hash.GetHashCode();
            }

            return hash;
        }
    }
}