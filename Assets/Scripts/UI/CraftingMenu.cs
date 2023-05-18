using System;
using System.Collections.Generic;
using Crafting;
using DataManager;
using Inventory;
using Items;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public class CraftingMenu : MonoBehaviour, IInventorySubscriber
    {
        [SerializeField] public TypeSprite[] itemTypeIcons;
        [SerializeField] public VisualTreeAsset button;
        [SerializeField] public VisualTreeAsset recipeListing;
        [SerializeField] public VisualTreeAsset ingredientRecipe;
        [SerializeField] private CraftingRecipeRegistryObject recipeRegistryObject;
        [SerializeField] private ItemRegistryObject itemRegistryObject;
        [SerializeField] private Player.Player player;
        public int currentCategoryIndex;
        public List<ItemType> tabCategories = new();
        public int currentRecipeIndex;

        public bool isCraftingMenuOpen;

        public readonly Dictionary<ItemType, Sprite> _itemTypeIcons = new();

        public readonly VisualElement[,] recipeViewGrid =
            new VisualElement[ItemConstants.ItemHeight, ItemConstants.ItemWidth];

        private readonly List<CraftingInterface> _interfaces = new();

        public VisualElement categoryTabs;
        public EventCallback<MouseUpEvent> createCallback;
        public IInventory inventory;
        public ItemRegistry itemRegistry;
        public VisualElement recipeCreateButton;
        public Label recipeDescription;
        public VisualElement recipeIngredients;
        public ListView recipeList;

        public VisualElement recipeListContainer;
        public VisualElement recipeListIcon;
        public Label recipeListName;
        public Label recipeName;

        public CraftingRecipeRegistry recipeRegistry;

        public VisualElement recipeView;
        public VisualElement root;

        private void Start()
        {
            foreach (TypeSprite typeSprite in itemTypeIcons)
            {
                _itemTypeIcons.Add(typeSprite.type, typeSprite.sprite);
            }

            inventory = player.GetInventory();
            player.GetInventoryNotifier().AddSubscriber(this);

            recipeRegistry = recipeRegistryObject.craftingRecipeRegistry;
            itemRegistry = itemRegistryObject.itemRegistry;

            root = GetComponent<UIDocument>().rootVisualElement;
            root.style.display = DisplayStyle.None;
            categoryTabs = root.Q<VisualElement>("CategoryTabs");

            recipeListContainer = root.Q<VisualElement>("RecipeListContainer");

            recipeListIcon = recipeListContainer.Q<VisualElement>("ListIcon");
            recipeListName = recipeListContainer.Q<Label>("ListTitleLabel");

            recipeView = root.Q<VisualElement>("RecipeView");
            recipeView.style.display = DisplayStyle.None;

            recipeName = recipeView.Q<Label>("RecipeName");
            recipeDescription = recipeView.Q<Label>("ItemDescription");

            for (int row = 1; row <= ItemConstants.ItemHeight; row++)
            {
                VisualElement rowElement = recipeView.Q<VisualElement>($"Row{row}");

                for (int col = 1; col <= ItemConstants.ItemWidth; col++)
                {
                    VisualElement colElement = rowElement.Q<VisualElement>($"Cell{col}");
                    recipeViewGrid[row - 1, col - 1] = colElement;
                }
            }

            recipeIngredients = recipeView.Q<VisualElement>("Ingredients");
            recipeCreateButton = recipeView.Q<VisualElement>("CreateButton");
        }

        public void OnInventoryChanged()
        {
            throw new NotImplementedException();
        }

        private void Open(MachineType type)
        {
            CraftingInterface categoryTabsInterface = new CategoryTabs(this, type);
            _interfaces.Clear();
            _interfaces.Add(categoryTabsInterface);

            categoryTabsInterface.Open();
        }

        private void Close()
        {
            foreach (CraftingInterface @interface in _interfaces)
            {
                @interface.Close();
            }

            _interfaces.Clear();
        }

        private void RefreshRecipeList()
        {
            recipeList.RefreshItems();
        }


        public void Toggle(MachineType type)
        {
            if (isCraftingMenuOpen)
            {
                root.style.display = DisplayStyle.None;
                Close();
            }
            else
            {
                root.style.display = DisplayStyle.Flex;
                Open(type);
            }

            isCraftingMenuOpen = !isCraftingMenuOpen;
        }

        [Serializable]
        public class TypeSprite
        {
            public ItemType type;
            public Sprite sprite;
        }
    }
}