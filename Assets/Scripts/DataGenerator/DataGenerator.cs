using Items;
using UnityEngine;

namespace DataGenerator
{
    /// <summary>
    /// Used only once to generate the ItemData.json file.
    /// This file is then used to load items into the game.
    /// Hence, this script is not needed in the final build.
    /// </summary>
    public class DataGenerator : MonoBehaviour
    {
        private void Start()
        {
            Item item = new Item("Test Item", "This is a test item", "test");

            string json = JsonUtility.ToJson(item);
            System.IO.File.WriteAllText("ItemData.json", json);
        }
    }
}
