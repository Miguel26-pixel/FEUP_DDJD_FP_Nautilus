using System.Collections.Generic;
using Crafting;
using Items;
using UnityEngine;

namespace DataManager
{
    public class CraftingRecipeRegistry : Registry<CraftingRecipe>
    {
        [SerializeField] private ItemRegistry itemRegistry;
        private readonly List<CraftingRecipe> _recipes = new();

        public CraftingRecipe CreateCraftingRecipe(string result, MachineType machineType,
            Dictionary<string, int> ingredients, int quantity = 1)
        {
            CraftingRecipe recipe = new(result, machineType, ingredients, quantity);
            _recipes.Add(recipe);

            return recipe;
        }

        public override void Add(CraftingRecipe item)
        {
            _recipes.Add(item);
        }

        public override CraftingRecipe[] GetAll()
        {
            return _recipes.ToArray();
        }

        public CraftingRecipe[] GetOfType(ItemType type, MachineType machineType)
        {
            List<CraftingRecipe> recipes = new();

            foreach (CraftingRecipe recipe in _recipes)
            {
                Item result = itemRegistry.Get(recipe.Result);

                if (result.Type == type && recipe.MachineType == machineType)
                {
                    recipes.Add(recipe);
                }
            }

            return recipes.ToArray();
        }
    }
}