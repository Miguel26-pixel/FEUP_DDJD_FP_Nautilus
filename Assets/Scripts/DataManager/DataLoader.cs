using System.Collections.Generic;
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

            Item[] items = JsonConvert.DeserializeObject<Item[]>(json);

            foreach (var item in items)
            {
                Debug.Log(item.Name);
                Debug.Log(item.Description);
                Debug.Log(item.ID);
                
                itemRegistry.Add(item);
                Debug.Log(itemRegistry.Get(item.ID).Name);
            }

            Debug.Log(itemRegistry.GetAll().Length);
        }
    }
}
