using System;
using System.Collections.Generic;
using Items;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace UI {

    public class CraftingMenu : MonoBehaviour
    {
        [Serializable]
        public class TypeSprite
        {
            public ItemType type;
            public Sprite sprite;
        }

        [SerializeField] private TypeSprite[] itemTypeIcons;
        [SerializeField] private VisualTreeAsset button;
        [SerializeField] private VisualTreeAsset recipeListing;
        [SerializeField] private VisualTreeAsset ingredientRecipe;
        
        private Dictionary<ItemType, Sprite> _itemTypeIcons = new();
        private VisualElement _categoryTabs;
        
        private VisualElement _recipeListContainer;
        private VisualElement _recipeListIcon;
        private VisualElement _recipeListName;
        private VisualElement _recipeList;

        private VisualElement _recipeView;
        private readonly VisualElement[,] _recipeViewGrid = new VisualElement[ItemConstants.ItemHeight, ItemConstants.ItemWidth]; 
        private VisualElement _recipeName;
        private VisualElement _recipeDescription;
        private VisualElement _recipeIngredients;
        private VisualElement _recipeCreateButton;
        
        private bool _isCraftingMenuOpen;
        private bool _isRecipeListingOpen;
        private bool _isRecipeViewOpen;

        private void Start()
        {
            foreach (TypeSprite typeSprite in itemTypeIcons)
            {
                _itemTypeIcons.Add(typeSprite.type, typeSprite.sprite);
            }
            
            VisualElement root = GetComponent<UIDocument>().rootVisualElement;
            root.style.display = DisplayStyle.None;
            _categoryTabs = root.Q<VisualElement>("CategoryTabs");
            
            _recipeListContainer = root.Q<VisualElement>("RecipeListContainer");
            _recipeListContainer.style.display = DisplayStyle.None;
            
            _recipeListIcon = _recipeListContainer.Q<VisualElement>("ListIcon");
            _recipeListName = _recipeListContainer.Q<VisualElement>("ListTitleName");
            _recipeList =  _recipeListContainer.Q<ListView>("RecipeList");
            
            _recipeView = root.Q<VisualElement>("RecipeView");
            _recipeView.style.display = DisplayStyle.None;
            
            _recipeName = _recipeView.Q<VisualElement>("RecipeName");
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
    }
}
