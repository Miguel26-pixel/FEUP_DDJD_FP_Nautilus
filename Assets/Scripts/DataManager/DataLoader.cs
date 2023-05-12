using Crafting;
using Items;
using Newtonsoft.Json;
using UnityEngine;

namespace DataManager
{
    public class DataLoader : MonoBehaviour
    {
        public ItemRegistry itemRegistry;
        public CraftingRecipeRegistry recipeRegistry;

        private void Start()
        {
            // Get JSON string from ItemData.json Asset
            TextAsset jsonAsset = Resources.Load<TextAsset>("ItemData");
            string json = jsonAsset.text;

            Item[] items = JsonConvert.DeserializeObject<Item[]>(json,
                new JsonSerializerSettings
                    { TypeNameHandling = TypeNameHandling.Auto, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

            foreach (Item item in items)
            {
                itemRegistry.Add(item);
            }

            itemRegistry.SetInitialized();
            
            TextAsset recipeAsset = Resources.Load<TextAsset>("RecipeData");
            string recipeJson = recipeAsset.text;
            
            CraftingRecipe[] recipes = JsonConvert.DeserializeObject<CraftingRecipe[]>(recipeJson,
                new JsonSerializerSettings
                    { TypeNameHandling = TypeNameHandling.Auto, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            
            foreach (CraftingRecipe recipe in recipes)
            {
                recipeRegistry.Add(recipe);
            }
            
            recipeRegistry.SetInitialized();
        }
    }
}