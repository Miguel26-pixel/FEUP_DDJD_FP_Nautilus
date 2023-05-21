using System.Collections.Generic;
using Crafting;
using DataManager;
using Inventory;
using Items;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class RecipeView : CraftingInterface
    {
        private readonly CraftingMenu _craftingMenu;
        private readonly VisualTreeAsset _ingredientRecipe;
        private readonly PlayerInventory _inventory;
        private readonly ItemRegistry _itemRegistry;
        private readonly CraftingRecipe _recipe;
        private readonly VisualElement _recipeCreateButton;
        private readonly Label _recipeDescription;
        private readonly VisualElement _recipeIngredients;
        private readonly Label _recipeName;
        private readonly VisualElement _recipeView;
        private readonly VisualElement[,] _recipeViewGrid;
        private readonly ItemData _resultItem;
        private EventCallback<MouseUpEvent> _createCallback;

        public RecipeView(CraftingMenu craftingMenu, CraftingRecipe recipe)
        {
            _itemRegistry = craftingMenu.itemRegistry;
            _recipe = recipe;
            _recipeName = craftingMenu.recipeName;
            _recipeDescription = craftingMenu.recipeDescription;
            _recipeViewGrid = craftingMenu.recipeViewGrid;
            _recipeIngredients = craftingMenu.recipeIngredients;
            _inventory = craftingMenu.inventory;
            _ingredientRecipe = craftingMenu.ingredientRecipe;
            _recipeCreateButton = craftingMenu.recipeCreateButton;
            _recipeView = craftingMenu.recipeView;
            _resultItem = _itemRegistry.Get(_recipe.Result);
            _craftingMenu = craftingMenu;
        }

        public override void Open()
        {
            _recipeName.text = _resultItem.Name;
            _recipeDescription.text = _resultItem.Description;

            for (int row = 0; row < ItemConstants.ItemHeight; row++)
            {
                for (int col = 0; col < ItemConstants.ItemWidth; col++)
                {
                    Sprite icon = _resultItem.Icons[row, col];
                    VisualElement cell = _recipeViewGrid[row, col];

                    if (icon == null)
                    {
                        cell.AddToClassList("unused");

                        VisualElement iconElement = cell.Q<VisualElement>("ItemIcon");
                        iconElement.style.backgroundImage = new StyleBackground();
                    }
                    else
                    {
                        cell.RemoveFromClassList("unused");

                        VisualElement iconElement = cell.Q<VisualElement>("ItemIcon");
                        iconElement.style.backgroundImage = new StyleBackground(icon);
                    }
                }
            }

            Refresh();
            _recipeView.style.display = DisplayStyle.Flex;
        }

        private void CraftItem(CraftingRecipe recipe, ItemData resultData)
        {
            if (!recipe.CorrectCount(recipe.IngredientCount(_inventory.GetItems())))
            {
                return;
            }

            _craftingMenu.isCrafting = true;
            foreach (KeyValuePair<int, int> ingredient in recipe.Ingredients)
            {
                for (int i = 0; i < ingredient.Value; i++)
                {
                    _inventory.RemoveItem(ingredient.Key);
                }
            }

            _craftingMenu.isCrafting = false;
            _craftingMenu.OnInventoryChanged();

            Item resultItem = resultData.CreateInstance();
            CraftingInventory craftingInventory = CraftingInventory.CreateCraftingInventory();

            // _inventory.TransferItems(craftingInventory, TransferDirection.DestinationToSource);
        }


        public override void Close()
        {
            if (_createCallback != null)
            {
                _recipeCreateButton.UnregisterCallback(_createCallback);
            }

            _recipeView.style.display = DisplayStyle.None;
        }

        public override void Refresh()
        {
            _recipeIngredients.Clear();
            Dictionary<int, int> ingredientCount = _recipe.IngredientCount(_inventory.GetItems());

            foreach (KeyValuePair<int, int> ingredient in _recipe.Ingredients)
            {
                ItemData ingredientItem = _itemRegistry.Get(ingredient.Key);
                VisualElement ingredientElement = _ingredientRecipe.Instantiate();
                VisualElement icon = ingredientElement.Q<VisualElement>("ListIcon");
                Label itemName = ingredientElement.Q<Label>("ItemName");
                Label currentCount = ingredientElement.Q<Label>("CurrentCount");
                Label requiredCount = ingredientElement.Q<Label>("RequiredCount");

                icon.style.backgroundImage = new StyleBackground(ingredientItem.Icon);
                itemName.text = ingredientItem.Name;
                currentCount.text = ingredientCount[ingredient.Key].ToString();
                requiredCount.text = ingredient.Value.ToString();

                ingredientElement.AddToClassList("ingredient");
                if (ingredientCount[ingredient.Key] < ingredient.Value)
                {
                    ingredientElement.AddToClassList("incomplete");
                }

                _recipeIngredients.Add(ingredientElement);
            }

            bool canCraft = _recipe.CorrectCount(ingredientCount);

            if (_createCallback != null)
            {
                _recipeCreateButton.UnregisterCallback(_createCallback);
            }

            if (canCraft)
            {
                _recipeCreateButton.RemoveFromClassList("disabled");
                _recipeCreateButton.RemoveFromClassList("incomplete");
                _createCallback = _ => { CraftItem(_recipe, _resultItem); };

                _recipeCreateButton.RegisterCallback(_createCallback);
            }
            else
            {
                _recipeCreateButton.AddToClassList("disabled");
                _recipeCreateButton.AddToClassList("incomplete");
                _createCallback = null;
            }

            _recipeCreateButton.Q<Label>("CreateText").text =
                _recipe.Quantity > 1 ? $"Create (x{_recipe.Quantity})" : "Create";
        }
    }
}