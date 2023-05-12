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

            foreach (Item item in items)
            {
                Debug.Log(item.Name);
                Debug.Log(item.Description);
                Debug.Log(item.ID);

                Debug.Log(item.HasComponent<WeaponComponent>());
                Debug.Log(item.GetComponent<WeaponComponent>().Damage);
                Debug.Log(item.GetComponent<WeaponComponent>().Range);
                Debug.Log(item.GetComponent<WeaponComponent>().AttackSpeed);
                Debug.Log(item.GetComponent<WeaponComponent>().Durability);
                Debug.Log(item.GetComponent<WeaponComponent>().Slot);

                itemRegistry.Add(item);
                Debug.Log(itemRegistry.Get(item.ID).Name);
            }

            Debug.Log(itemRegistry.GetAll().Length);
            itemRegistry.SetInitialized();
        }
    }
}