using Items;
using Newtonsoft.Json;
using UnityEngine;

namespace DataManager
{
    public class DataLoader : MonoBehaviour
    {
        public ItemRegistry itemRegistry;

        private void Start()
        {
            // Get JSON string from ItemData.json Asset
            TextAsset jsonAsset = Resources.Load<TextAsset>("ItemData");
            string json = jsonAsset.text;

            Item[] items = JsonConvert.DeserializeObject<Item[]>(json,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

            foreach (Item item in items) itemRegistry.Add(item);

            itemRegistry.SetInitialized();
        }
    }
}