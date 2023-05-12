using System;
using System.Collections.Generic;
using Crafting;
using Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace DataManager
{
    public class CraftingRecipeRegistry : Registry<CraftingRecipe>
    {
        private List<CraftingRecipe> _recipes; 
        [SerializeField] private ItemRegistry itemRegistry;
        
        public CraftingRecipe CreateCraftingRecipe(string result, MachineType machineType, Dictionary<string, int> ingredients)
        {
            CraftingRecipe recipe = new CraftingRecipe(result, machineType, ingredients);
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
            List<CraftingRecipe> recipes = new List<CraftingRecipe>();
            
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