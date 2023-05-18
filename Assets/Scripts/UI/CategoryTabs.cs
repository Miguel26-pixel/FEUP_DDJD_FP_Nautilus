using System;
using System.Collections.Generic;
using Crafting;
using DataManager;
using Items;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class CategoryTabs : CraftingInterface
    {
        private readonly VisualTreeAsset _button;
        private readonly VisualElement _categoryTabs;
        private readonly Dictionary<ItemType, Sprite> _itemTypeIcons;

        private readonly CraftingRecipeRegistry _recipeRegistry;
        private readonly List<ItemType> _tabCategories = new();
        private readonly MachineType _type;
        private readonly CraftingMenu _craftingMenu;
        private int _currentCategoryIndex;

        private CraftingInterface _recipeListing;

        public CategoryTabs(CraftingMenu craftingMenu, MachineType type)
        {
            _craftingMenu = craftingMenu;
            _type = type;
            _recipeRegistry = craftingMenu.recipeRegistry;
            _categoryTabs = craftingMenu.categoryTabs;
            _button = craftingMenu.button;
            _itemTypeIcons = craftingMenu._itemTypeIcons;
            _currentCategoryIndex = 0;
        }

        public override void Open()
        {
            SortedSet<ItemType> categories = _recipeRegistry.AvailableCategories(_type);
            _categoryTabs.Clear();
            _tabCategories.Clear();
            int idx = 0;

            foreach (ItemType category in categories)
            {
                _tabCategories.Add(category);

                VisualElement tab = _button.Instantiate();
                tab.AddToClassList("tab");

                VisualElement icon = tab.Q<VisualElement>("Icon");
                icon.style.backgroundImage = new StyleBackground(_itemTypeIcons[category]);

                int categoryIndex = idx;
                tab.RegisterCallback<MouseUpEvent>(_ => OpenRecipeListing(categoryIndex, _type));
                _categoryTabs.Add(tab);
                idx++;
            }

            OpenRecipeListing(0, _type);
        }

        public override void Close()
        {
            _recipeListing?.Close();
        }

        private void OpenRecipeListing(int categoryIndex, MachineType type)
        {
            _recipeListing?.Close();

            _categoryTabs[_currentCategoryIndex].RemoveFromClassList("selected");
            _currentCategoryIndex = categoryIndex;
            _categoryTabs[_currentCategoryIndex].AddToClassList("selected");
            ItemType category = _tabCategories[_currentCategoryIndex];

            _recipeListing = new RecipeListing(_craftingMenu, category, type);
            _recipeListing.Open();
        }


        public override void Refresh()
        {
            _recipeListing.Refresh();
        }
    }
}