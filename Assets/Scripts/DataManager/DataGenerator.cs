using System.IO;
using Items;
using Newtonsoft.Json;
using UnityEngine;

namespace DataManager
{
    /// <summary>
    ///     Used only once to generate the ItemData.json file.
    ///     This file is then used to load items into the game.
    ///     Hence, this script is not needed in the final build.
    /// </summary>
    public class DataGenerator : MonoBehaviour
    {
        public ItemRegistry itemRegistry;

        private void Start()
        {
            Item spear = itemRegistry.CreateItem("Spear",
                "The perfect tool for when you want to stab something, but don't want to get too close.",
                "ItemIcons/test");

            WeaponComponent weaponComponent = new(0, 30, 10, 5, 1);
            spear.AddComponent(weaponComponent);

            Item[] items = itemRegistry.GetAll();
            string json = JsonConvert.SerializeObject(items,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            File.WriteAllText("ItemData.json", json);
        }
    }
}