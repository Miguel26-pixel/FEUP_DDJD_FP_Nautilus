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
        [JsonProperty("ingredients")] private Dictionary<string, int> _ingredientsHex;

        [JsonProperty("result")] private string _resultHex;

        public CraftingRecipe(string result, MachineType machineType, Dictionary<string, int> ingredients,
            int quantity = 1)
        {
            _ingredientsHex = ingredients;
            _resultHex = result;
            MachineType = machineType;
            Quantity = quantity;

            // hex string to int
            Result = int.Parse(result, NumberStyles.HexNumber);
            Ingredients = new Dictionary<int, int>();

            foreach (KeyValuePair<string, int> ingredient in ingredients)
            {
                Ingredients.Add(int.Parse(ingredient.Key, NumberStyles.HexNumber), ingredient.Value);
            }
        }

        // The first int is the item id, the second int is the amount
        [JsonIgnore] public Dictionary<int, int> Ingredients { get; }

        [JsonIgnore] public int Result { get; }

        [JsonProperty("machineType")] public MachineType MachineType { get; }
        [JsonProperty("quantity")] public int Quantity { get; }


        public bool CanCraft(MachineType machineType, List<Item> items)
        {
            if (!CanCraftOnMachine(machineType))
            {
                return false;
            }

            // Check if the player has the required items, and if they have enough of them
            Dictionary<int, int> count = new();

            foreach (KeyValuePair<int, int> ingredient in Ingredients)
            {
                count.Add(ingredient.Key, 0);
            }

            foreach (Item item in items)
            {
                if (Ingredients.ContainsKey(item.IDHash))
                {
                    count[item.IDHash]++;
                }
            }

            foreach (KeyValuePair<int, int> ingredient in Ingredients)
            {
                if (count[ingredient.Key] < ingredient.Value)
                {
                    return false;
                }
            }

            return true;
        }
        
        public bool CanCraftOnMachine(MachineType machineType) => (MachineType & machineType) != 0;
    }
}