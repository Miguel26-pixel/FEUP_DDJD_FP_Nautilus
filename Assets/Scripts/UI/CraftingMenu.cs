using System;
using System.Collections.Generic;
using System.Linq;
using Crafting;
using DataManager;
using Items;
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
        private CraftingRecipeRegistry _recipeRegistry;
        private ItemRegistry _itemRegistry;

        private readonly Dictionary<ItemType, Sprite> _itemTypeIcons = new();

        private readonly VisualElement[,] _recipeViewGrid =
            new VisualElement[ItemConstants.ItemHeight, ItemConstants.ItemWidth];

        private VisualElement _categoryTabs;
        private int _currentCategoryIndex;
        private List<ItemType> _tabCategories = new();
        private VisualElement _root;

        private bool _isCraftingMenuOpen;
        private bool _isRecipeListingOpen;
        private bool _isRecipeViewOpen;
        private VisualElement _recipeCreateButton;
        private VisualElement _recipeDescription;
        private VisualElement _recipeIngredients;

        private VisualElement _recipeListContainer;
        private VisualElement _recipeListIcon;
        private Label _recipeListName;
        private Label _recipeName;

        private VisualElement _recipeView;

        private void Start()
        {
            foreach (TypeSprite typeSprite in itemTypeIcons)
            {
                _itemTypeIcons.Add(typeSprite.type, typeSprite.sprite);
            }

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
            _recipeDescription = _recipeView.Q<VisualElement>("ItemDescription");

            for (int row = 1; row <= ItemConstants.ItemHeight; row++)
            {
                VisualElement rowElement = _recipeView.Q<VisualElement>($"Row{row}");

                for (int col = 1; col <= ItemConstants.ItemWidth; col++)
                {
                    VisualElement colElement = rowElement.Q<VisualElement>($"Cell{col}");
                    _recipeViewGrid[row - 1, col - 1] = colElement;
                }
            }

            _recipeIngredients = _recipeView.Q<VisualElement>("RecipeIngredients");
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
                tab.RegisterCallback<MouseUpEvent>(evt => OpenRecipeListing(categoryIndex, type));
                _categoryTabs.Add(tab);
                idx++;
            }
            OpenRecipeListing(0, type);
        }

        private void OpenRecipeListing(int categoryIndex, MachineType type)
        {
            _categoryTabs[_currentCategoryIndex].RemoveFromClassList("selected");
            _currentCategoryIndex = categoryIndex;
            _categoryTabs[_currentCategoryIndex].AddToClassList("selected");
            ItemType category = _tabCategories[_currentCategoryIndex];
            
            _recipeListIcon.style.backgroundImage = new StyleBackground(_itemTypeIcons[category]);
            _recipeListName.text = category.ToString();

            List<CraftingRecipe> recipes = _recipeRegistry.GetOfType(category, type).ToList();

            VisualElement MakeItem() => recipeListing.Instantiate();

            void BindItem(VisualElement recipe, int idx)
            {
                VisualElement icon = recipe.Q<VisualElement>("Icon");
                Label itemName = recipe.Q<Label>("ItemName");

                // TODO: convert icon grid to a single icon
                icon.style.backgroundImage = new StyleBackground(_itemTypeIcons[category]);
                itemName.text = _itemRegistry.Get(recipes[idx].Result).Name;
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
            
            _recipeListContainer.Add(recipeList);
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