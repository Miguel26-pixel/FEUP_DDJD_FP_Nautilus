using UnityEngine;

namespace DataManager
{
    public class CraftingRecipeRegistryObject : MonoBehaviour
    {
        public ItemRegistryObject itemRegistryObject;
        public CraftingRecipeRegistry craftingRecipeRegistry;

        private void Awake()
        {
            craftingRecipeRegistry = new CraftingRecipeRegistry(itemRegistryObject.itemRegistry);
        }
    }
}