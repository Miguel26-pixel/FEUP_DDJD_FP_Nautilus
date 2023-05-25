using System;
using System.Collections.Generic;
using Crafting;
using DataManager;
using Inventory;
using Items;
using UI.Inventory;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Crafting
{
    public class CraftingMenu : MonoBehaviour, IInventorySubscriber
    {
        [SerializeField] public Sprite crossIcon;
        [SerializeField] public TransferInventoryMenu transferInventoryMenu;
        [SerializeField] public TypeSprite[] itemTypeIcons;
        [SerializeField] public VisualTreeAsset button;
        [SerializeField] public VisualTreeAsset recipeListing;
        [SerializeField] public VisualTreeAsset ingredientRecipe;
        [SerializeField] private CraftingRecipeRegistryObject recipeRegistryObject;
        [SerializeField] private ItemRegistryObject itemRegistryObject;
        [SerializeField] public Player player;
        private readonly List<CraftingInterface> _interfaces = new();
        public readonly Dictionary<ItemType, Sprite> _itemTypeIcons = new();

        [NonSerialized] public readonly VisualElement[,] recipeViewGrid =
            new VisualElement[ItemConstants.ItemHeight, ItemConstants.ItemWidth];

        [NonSerialized] private bool _isCraftingMenuOpen;
        private VisualElement _root;

        [NonSerialized] public VisualElement categoryTabs;
        public PlayerInventory Inventory => player.GetInventory();

        [NonSerialized] public ItemRegistry itemRegistry;
        [NonSerialized] public VisualElement recipeCreateButton;
        [NonSerialized] public Label recipeDescription;
        [NonSerialized] public VisualElement recipeIngredients;

        [NonSerialized] public VisualElement recipeListContainer;
        [NonSerialized] public VisualElement recipeListIcon;
        [NonSerialized] public Label recipeListName;
        [NonSerialized] public Label recipeName;

        [NonSerialized] public CraftingRecipeRegistry recipeRegistry;

        [NonSerialized] public VisualElement recipeView;

        [NonSerialized] public bool isCrafting;

        private void Start()
        {
            foreach (TypeSprite typeSprite in itemTypeIcons)
            {
                _itemTypeIcons.Add(typeSprite.type, typeSprite.sprite);
            }

            player.GetInventoryNotifier().AddSubscriber(this);

            recipeRegistry = recipeRegistryObject.craftingRecipeRegistry;
            itemRegistry = itemRegistryObject.itemRegistry;

            _root = GetComponent<UIDocument>().rootVisualElement;
            _root.style.display = DisplayStyle.None;
            categoryTabs = _root.Q<VisualElement>("CategoryTabs");

            recipeListContainer = _root.Q<VisualElement>("RecipeListContainer");

            recipeListIcon = recipeListContainer.Q<VisualElement>("ListIcon");
            recipeListName = recipeListContainer.Q<Label>("ListTitleLabel");

            recipeView = _root.Q<VisualElement>("RecipeView");
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

        public void UpdateInventory()
        {
            player.GetInventoryNotifier().AddSubscriber(this);
            
            OnInventoryChanged();
        }

        public void OnInventoryChanged()
        {
            if (isCrafting)
            {
                return;
            }
            
            foreach (CraftingInterface @interface in _interfaces)
            {
                @interface.Refresh();
            }
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

        public void Toggle(MachineType type)
        {
            if (_isCraftingMenuOpen)
            {
                _root.style.display = DisplayStyle.None;
                Close();
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