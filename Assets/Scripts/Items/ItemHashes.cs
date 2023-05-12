using System.Collections.Generic;
using System.Globalization;

namespace Items
{
    public static class ItemRegistry
    {
        private static readonly Dictionary<int, Item> Items = new();
        
        /// <summary>
        /// Adds the item to the dictionary and returns the hash as a string.
        /// </summary>
        public static string Add(Item item)
        {
            int hash = Hash(item);
            Items.Add(hash, item);
            
            return hash.ToString("X");
        }

        /// <summary>
        /// Adds item with known hash to the dictionary.
        /// Used when loading items from JSON.
        /// </summary>
        public static void Add(string hash, Item item)
        {
            int key = int.Parse(hash, NumberStyles.HexNumber);
            Items.Add(key, item);
        }
        
        
        /// <summary>
        /// Gets the item with the given string hash.
        /// The use of a string hash is for better readability in the JSON file.
        /// </summary>
        /// <param name="hash">The string hash equivalent of the item's ID</param>
        public static Item Get(string hash)
        {
            int key = int.Parse(hash, NumberStyles.HexNumber);
            return Items[key];
        }
        
        /// <summary>
        /// Hashes the item's name and description to generate a unique ID.
        /// </summary>
        private static int Hash(Item item) {
            string hashString = item.Name + item.Description;
            
            int hash = hashString.GetHashCode();
            while (Items.ContainsKey(hash))
            {
                hash = hash.GetHashCode();
            }

            return hash;
        }
    }
}