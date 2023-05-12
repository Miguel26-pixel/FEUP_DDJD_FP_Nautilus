using System.Collections.Generic;
using Items;
using Newtonsoft.Json;
using UnityEngine;

namespace DataManager
{
    /// <summary>
    /// Used only once to generate the ItemData.json file.
    /// This file is then used to load items into the game.
    /// Hence, this script is not needed in the final build.
    /// </summary>
    public class DataGenerator : MonoBehaviour
    {
        public ItemRegistry itemRegistry;
        
        private void Start()
        {
            itemRegistry.CreateItem("Test Item", "This is a test item.", "ItemIcons/test");

            Item[] items = itemRegistry.GetAll();
            string json = JsonConvert.SerializeObject(items);
            System.IO.File.WriteAllText("ItemData.json", json);
        }
    }
}
