using System;
using System.Collections.Generic;
using System.Globalization;
using Items;
using Newtonsoft.Json;

namespace Crafting
{
    [Serializable]
    public class CraftingRecipe
    {
        // The first int is the item id, the second int is the amount
        [JsonIgnore]
        private Dictionary<int, int> Ingredients { get; }
        [JsonProperty("ingredients")]
        private Dictionary<string, int> _ingredientsHex;

        public CraftingRecipe(Dictionary<string, int> ingredients)
        {
            // hex string to int
            Ingredients = new Dictionary<int, int>();
            _ingredientsHex = ingredients;
            
            foreach (var ingredient in ingredients)
            {
                Ingredients.Add(int.Parse(ingredient.Key, NumberStyles.HexNumber), ingredient.Value);
            }
        }
        
        public bool CanCraft(List<Item> items)
        {
            // Check if the player has the required items, and if they have enough of them
            Dictionary<int, int> count = new Dictionary<int, int>();

            foreach (var ingredient in Ingredients)
            {
                count.Add(ingredient.Key, 0);
            }
            
            foreach (var item in items)
            {
                if (Ingredients.ContainsKey(item.IDHash))
                {
                    count[item.IDHash]++;
                }
            }
            
            foreach (var ingredient in Ingredients)
            {
                if (count[ingredient.Key] < ingredient.Value)
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}