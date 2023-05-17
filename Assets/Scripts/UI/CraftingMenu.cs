using System;
using System.Collections.Generic;
using System.Linq;
using Crafting;
using DataManager;
using Items;
using Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class CraftingMenu : MonoBehaviour
    {
        [SerializeField] private TypeSprite[] itemTypeIcons;
        [SerializeField] private VisualTreeAsset button;
        [SerializeField] private VisualTreeAsset recipeListing;
        [SerializeField] private VisualTreeAsset ingredientRecipe;
        [SerializeField] private CraftingRecipeRegistryObject recipeRegistryObject;
        [SerializeField] private ItemRegistryObject itemRegistryObject;
        [SerializeField] private Player.Player player;

        private CraftingRecipeRegistry _recipeRegistry;
        private ItemRegistry _itemRegistry;
        private IInventory _inventory;

        private readonly Dictionary<ItemType, Sprite> _itemTypeIcons = new();

        private readonly VisualElement[,] _recipeViewGrid =
            new VisualElement[ItemConstants.ItemHeight, ItemConstants.ItemWidth];

        private VisualElement _categoryTabs;
        private int _currentCategoryIndex;
        private List<ItemType> _tabCategories = new();
        private VisualElement _root;
        private int _currentRecipeIndex;

        private bool _isCraftingMenuOpen;
        private bool _isRecipeListingOpen;
        private bool _isRecipeViewOpen;
        private VisualElement _recipeCreateButton;
        private VisualElement _recipeIngredients;
        private Label _recipeDescription;
        private Label _recipeName;

        private VisualElement _recipeListContainer;
        private VisualElement _recipeListIcon;
        private ListView _recipeList;
        private Label _recipeListName;

        private VisualElement _recipeView;

        private void Start()
        {
            foreach (TypeSprite typeSprite in itemTypeIcons)
            {
                _itemTypeIcons.Add(typeSprite.type, typeSprite.sprite);
            }
            _inventory = player.GetInventory();

            _recipeRegistry = recipeRegistryObject.craftingRecipeRegistry;
            _itemRegistry = itemRegistryObject.itemRegistry;

            _root = GetComponent<UIDocument>().rootVisualElement;
            _root.style.display = DisplayStyle.None;
            _categoryTabs = _root.Q<VisualElement>("CategoryTabs");

            _recipeListContainer = _root.Q<VisualElement>("RecipeListContainer");

            _recipeListIcon = _recipeListContainer.Q<VisualElement>("ListIcon");
            _recipeListName = _recipeListContainer.Q<Label>("ListTitleLabel");

            _recipeView = _root.Q<VisualElement>("RecipeView");
            _recipeView.style.display = DisplayStyle.None;

            _recipeName = _recipeView.Q<Label>("RecipeName");
            _recipeDescription = _recipeView.Q<Label>("ItemDescription");

            for (int row = 1; row <= ItemConstants.ItemHeight; row++)
            {
                VisualElement rowElement = _recipeView.Q<VisualElement>($"Row{row}");

                for (int col = 1; col <= ItemConstants.ItemWidth; col++)
                {
                    VisualElement colElement = rowElement.Q<VisualElement>($"Cell{col}");
                    _recipeViewGrid[row - 1, col - 1] = colElement;
                }
            }

            _recipeIngredients = _recipeView.Q<VisualElement>("Ingredients");
            _recipeCreateButton = _recipeView.Q<VisualElement>("CreateButton");
        }

        private void Open(MachineType type)
        {
            var categories = _recipeRegistry.AvailableCategories(type);
            _categoryTabs.Clear();
            _tabCategories.Clear();
            int idx = 0;
            
            foreach (ItemType category in categories)
            {
                _tabCategories.Add(category);
                
                VisualElement tab = button.Instantiate();
                tab.AddToClassList("tab");

                VisualElement icon = tab.Q<VisualElement>("Icon");
                icon.style.backgroundImage = new StyleBackground(_itemTypeIcons[category]);

                int categoryIndex = idx;
                tab.RegisterCallback<MouseUpEvent>(_ => OpenRecipeListing(categoryIndex, type));
                _categoryTabs.Add(tab);
                idx++;
            }
            OpenRecipeListing(0, type);
        }

        private void OpenRecipeListing(int categoryIndex, MachineType type)
        {
            _currentRecipeIndex = -1;
            _recipeView.style.display = DisplayStyle.None;
            _categoryTabs[_currentCategoryIndex].RemoveFromClassList("selected");
            _currentCategoryIndex = categoryIndex;
            _categoryTabs[_currentCategoryIndex].AddToClassList("selected");
            ItemType category = _tabCategories[_currentCategoryIndex];
            
            _recipeListIcon.style.backgroundImage = new StyleBackground(_itemTypeIcons[category]);
            _recipeListName.text = category.ToString();

            List<CraftingRecipe> recipes = _recipeRegistry.GetOfType(category, type).ToList();

            VisualElement MakeItem()
            {
                VisualElement recipe = recipeListing.Instantiate();
                VisualElement button = recipe.Q<VisualElement>("Button");

                button.RegisterCallback<MouseUpEvent>(_ =>
                {
                    int idx = recipe.userData as int? ?? -1;
                    OpenRecipeView(recipes[idx], idx);
                });

                return recipe;
            }

            void BindItem(VisualElement recipe, int idx)
            {
                VisualElement icon = recipe.Q<VisualElement>("Icon");
                Label itemName = recipe.Q<Label>("ItemName");
                VisualElement button = recipe.Q<VisualElement>("Button");
                VisualElement item = recipe.Q<VisualElement>("Item");
                ItemData result = _itemRegistry.Get(recipes[idx].Result);

                icon.style.backgroundImage = new StyleBackground(result.Icon);
                button.pickingMode = PickingMode.Position;
                itemName.text = result.Name;
                
                if (idx == _currentRecipeIndex)
                {
                    button.AddToClassList("selected");
                }
                else
                {
                    button.RemoveFromClassList("selected");
                }
                
                recipe.userData = idx;
                
                
                if (recipes[idx].CanCraft(type, _inventory.GetItems()))
                {
                    item.RemoveFromClassList("incomplete");
                }
                else
                {
                    item.AddToClassList("incomplete");
                }
            }

            ListView recipeList = _recipeListContainer.Q<ListView>("RecipeList");

            if (recipeList != null)
            {
                _recipeListContainer.Remove(recipeList);
            }
            
            recipeList = new ListView(recipes, 75, MakeItem, BindItem)
            {
                style =
                {
                    flexGrow = 1
                },
                name = "RecipeList"
            };
            recipeList.AddToClassList("recipe-list");
            recipeList.selectionType = SelectionType.None;
            recipeList.pickingMode = PickingMode.Ignore;

            _recipeList = recipeList;
            _recipeListContainer.Add(recipeList);
        }

        private void OpenRecipeView(CraftingRecipe recipe, int index)
        {
            int oldRecipeIndex = _currentRecipeIndex;
            _currentRecipeIndex = index;
            if(oldRecipeIndex != -1) _recipeList.RefreshItem(oldRecipeIndex);
            _recipeList.RefreshItem(_currentRecipeIndex);

            ItemData resultItem = _itemRegistry.Get(recipe.Result);
            
            _recipeName.text = resultItem.Name;
            _recipeDescription.text = resultItem.Description;
            
            for (int row = 0; row < ItemConstants.ItemHeight; row++)
            {
                for (int col = 0; col < ItemConstants.ItemWidth; col++)
                {
                    Sprite icon = resultItem.Icons[row, col];
                    VisualElement cell = _recipeViewGrid[row, col];

                    if (icon == null)
                    {
                        cell.AddToClassList("disabled");
                        
                        VisualElement iconElement = cell.Q<VisualElement>("ItemIcon");
                        iconElement.style.backgroundImage = new StyleBackground();
                    }
                    else
                    {
                        cell.RemoveFromClassList("disabled");
                        
                        VisualElement iconElement = cell.Q<VisualElement>("ItemIcon");
                        iconElement.style.backgroundImage = new StyleBackground(icon);
                    }
                }
            }
            
            _recipeIngredients.Clear();
            Dictionary<int, int> ingredientCount = recipe.IngredientCount(_inventory.GetItems());

            foreach (var ingredient in recipe.Ingredients)
            {
                ItemData ingredientItem = _itemRegistry.Get(ingredient.Key);
                VisualElement ingredientElement = ingredientRecipe.Instantiate();
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

            bool canCraft = recipe.CorrectCount(ingredientCount);

            if (canCraft)
            {
                _recipeCreateButton.RemoveFromClassList("disabled");
                _recipeCreateButton.RemoveFromClassList("incomplete");
            }
            else
            {
                _recipeCreateButton.AddToClassList("disabled");
                _recipeCreateButton.AddToClassList("incomplete");
            }
            
            _recipeCreateButton.Q<Label>("CreateText").text = recipe.Quantity > 1 ? $"Create (x{recipe.Quantity})" : "Create";
            _recipeView.style.display = DisplayStyle.Flex;
        }

        public void Toggle(MachineType type)
        {
            if (_isCraftingMenuOpen)
            {
                _root.style.display = DisplayStyle.None;
            }
            else
            {
                _root.style.display = DisplayStyle.Flex;
                Open(type);
            }

            _isCraftingMenuOpen = !_isCraftingMenuOpen;
        }

        [Serializable]
        public class TypeSprite
        {
            public ItemType type;
            public Sprite sprite;
        }
    }
}