using System.Collections.Generic;
using Crafting;
using DataManager;
using FMODUnity;
using Inventory;
using Items;
using UI.Inventory;
using UI.Inventory.Builders;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Crafting
{
    public class RecipeView : CraftingInterface
    {
        private readonly CraftingMenu _craftingMenu;
        private readonly VisualTreeAsset _ingredientRecipe;
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
            if (!recipe.CorrectCount(recipe.IngredientCount(_craftingMenu.Inventory.GetItems())))
            {
                return;
            }

            PlayerInventory inventoryCopy = new(_craftingMenu.Inventory);
            bool hasSpace = HasSpace(recipe, inventoryCopy);
            Item resultItem = resultData.CreateInstance();

            if (hasSpace)
            {
                _craftingMenu.isCrafting =
                    true; // This is to prevent the crafting menu from updating while we're crafting
                foreach (KeyValuePair<int, int> ingredient in recipe.Ingredients)
                {
                    for (int i = 0; i < ingredient.Value; i++)
                    {
                        _craftingMenu.Inventory.RemoveItem(ingredient.Key);
                    }
                }

                _craftingMenu.isCrafting = false;
                RuntimeManager.PlayOneShot(_craftingMenu.craftSound);

                _craftingMenu.Inventory.AddItem(resultItem);
            }
            else
            {
                _craftingMenu.Inventory.Locked = true;

                CraftingInventory craftingInventory = CraftingInventory.CreateCraftingInventory();
                craftingInventory.AddItem(resultItem);

                GridInventoryViewerBuilder playerInventoryViewerBuilder =
                    new(inventoryCopy, _craftingMenu.player, canOpenContextMenu: false, refreshAfterMove: true);
                GridInventoryViewerBuilder craftingBuilder = new(craftingInventory, _craftingMenu.player, canOpenContextMenu: false);

                _craftingMenu.transferInventoryMenu.Open(
                    playerInventoryViewerBuilder,
                    craftingBuilder,
                    TransferDirection.DestinationToSource,
                    new List<TransferAction>
                    {
                        new(
                            new List<string> { "red-tint" },
                            _craftingMenu.crossIcon,
                            () =>
                            {
                                _craftingMenu.Inventory.Locked = false;
                                _craftingMenu.transferInventoryMenu.Close();
                            })
                    },
                    (_, right) => right.GetItems().Count == 0,
                    (grid, _) =>
                    {
                        _craftingMenu.player.SetInventory(new PlayerInventory(grid));
                        RuntimeManager.PlayOneShot(_craftingMenu.craftSound);
                        _craftingMenu.UpdateInventory();
                    });
            }
        }

        private bool HasSpace(CraftingRecipe recipe)
        {
            PlayerInventory inventoryCopy = new(_craftingMenu.Inventory);
            return HasSpace(recipe, inventoryCopy);
        }


        private bool HasSpace(CraftingRecipe recipe, InventoryGrid inventoryCopy)
        {
            Item resultItem = _itemRegistry.Get(recipe.Result).CreateInstance();

            foreach (KeyValuePair<int, int> ingredient in recipe.Ingredients)
            {
                for (int i = 0; i < ingredient.Value; i++)
                {
                    inventoryCopy.RemoveItem(ingredient.Key);
                }
            }

            try
            {
                inventoryCopy.FindEmptyPosition(resultItem, 0);
                return true;
            }
            catch (ItemDoesNotFitException)
            {
                return false;
            }
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
            Dictionary<int, int> ingredientCount = _recipe.IngredientCount(_craftingMenu.Inventory.GetItems());

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
            string postfix = "";

            if (_createCallback != null)
            {
                _recipeCreateButton.UnregisterCallback(_createCallback);
            }

            _recipeCreateButton.RemoveFromClassList("warning");

            if (canCraft)
            {
                bool hasSpace = HasSpace(_recipe);

                _recipeCreateButton.RemoveFromClassList("disabled");
                _recipeCreateButton.RemoveFromClassList("incomplete");

                if (!hasSpace)
                {
                    _recipeCreateButton.AddToClassList("warning");
                    postfix = " (Inventory Full)";
                }
                else
                {
                    _recipeCreateButton.RemoveFromClassList("warning");
                }

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
                _recipe.Quantity > 1 ? $"Create (x{_recipe.Quantity})" : "Create" + postfix;
        }
    }
}