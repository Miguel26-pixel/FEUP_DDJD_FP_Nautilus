using System.Collections.Generic;
using Crafting;
using Items;
using Newtonsoft.Json;
using UnityEngine;

namespace DataManager
{
    public static class DataDeserializer
    {
        public static IEnumerable<ItemData> DeserializeItemData(string json)
        {
            return JsonConvert.DeserializeObject<ItemData[]>(json,
                new JsonSerializerSettings
                    { TypeNameHandling = TypeNameHandling.Auto, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        public static IEnumerable<CraftingRecipe> DeserializeRecipeData(string json)
        {
            return JsonConvert.DeserializeObject<CraftingRecipe[]>(json,
                new JsonSerializerSettings
                    { TypeNameHandling = TypeNameHandling.Auto, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }

    public class DataLoader : MonoBehaviour
    {
        public ItemRegistryObject itemRegistryObject;

        public CraftingRecipeRegistryObject recipeRegistryObject;

        private ItemRegistry _itemRegistry;
        private CraftingRecipeRegistry _recipeRegistry;

        private void Start()
        {
            _itemRegistry = itemRegistryObject.itemRegistry;
            _recipeRegistry = recipeRegistryObject.craftingRecipeRegistry;

            // Get JSON string from ItemData.json Asset
            TextAsset jsonAsset = Resources.Load<TextAsset>("ItemData");
            string json = jsonAsset.text;
            IEnumerable<ItemData> items = DataDeserializer.DeserializeItemData(json);

            foreach (ItemData item in items)
            {
                _itemRegistry.Add(item);
            }

            _itemRegistry.SetInitialized();

            TextAsset recipeAsset = Resources.Load<TextAsset>("RecipeData");
            string recipeJson = recipeAsset.text;
            IEnumerable<CraftingRecipe> recipes = DataDeserializer.DeserializeRecipeData(recipeJson);

            foreach (CraftingRecipe recipe in recipes)
            {
                _recipeRegistry.Add(recipe);
            }

            _recipeRegistry.SetInitialized();
        }
    }
}