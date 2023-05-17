using System.Collections.Generic;
using System.Linq;
using Crafting;
using Items;

namespace DataManager
{
    public class CraftingRecipeRegistry
    {
        private readonly ItemRegistry _itemRegistry;
        private readonly List<CraftingRecipe> _recipes = new();

        public CraftingRecipeRegistry(ItemRegistry itemRegistry)
        {
            _itemRegistry = itemRegistry;
        }

        public bool Initialized { get; private set; }

        public void SetInitialized()
        {
            Initialized = true;
        }

        public CraftingRecipe CreateCraftingRecipe(string result, MachineType machineType,
            Dictionary<string, int> ingredients, int quantity = 1)
        {
            CraftingRecipe recipe = new(result, machineType, ingredients, quantity);
            _recipes.Add(recipe);

            return recipe;
        }

        public void Add(CraftingRecipe item)
        {
            _recipes.Add(item);
        }

        public IEnumerable<CraftingRecipe> GetAll()
        {
            return _recipes.ToArray();
        }

        public SortedSet<ItemType> AvailableCategories(MachineType machineType)
        {
            SortedSet<ItemType> categories = new();
            
            foreach (CraftingRecipe recipe in _recipes)
            {
                if (recipe.CanCraftOnMachine(machineType))
                {
                    ItemData result = _itemRegistry.Get(recipe.Result);
                    categories.Add(result.Type);
                }
            }

            return categories;
        }

        public CraftingRecipe[] GetOfType(ItemType type, MachineType machineType)
        {
            List<CraftingRecipe> recipes = new();

            foreach (CraftingRecipe recipe in _recipes)
            {
                ItemData result = _itemRegistry.Get(recipe.Result);

                if (result.Type == type && recipe.CanCraftOnMachine(machineType))
                {
                    recipes.Add(recipe);
                }
            }

            return recipes.ToArray();
        }
    }
}