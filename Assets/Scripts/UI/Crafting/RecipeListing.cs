using System.Collections.Generic;
using System.Linq;
using Crafting;
using DataManager;
using Items;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Crafting
{
    public class RecipeListing : CraftingInterface
    {
        private readonly CraftingMenu _craftingMenu;
        private readonly ItemRegistry _itemRegistry;
        private readonly ItemType _itemType;
        private readonly Dictionary<ItemType, Sprite> _itemTypeIcons;
        private readonly MachineType _machineType;
        private readonly VisualElement _recipeListContainer;
        private readonly VisualElement _recipeListIcon;
        private readonly VisualTreeAsset _recipeListing;
        private readonly Label _recipeListName;
        private readonly CraftingRecipeRegistry _recipeRegistry;
        private int _currentRecipeIndex;
        private ListView _recipeList;
        private RecipeView _recipeView;

        public RecipeListing(CraftingMenu craftingMenu, ItemType itemType, MachineType machineType)
        {
            _craftingMenu = craftingMenu;
            _itemType = itemType;
            _machineType = machineType;
            _itemTypeIcons = craftingMenu._itemTypeIcons;
            _recipeListIcon = craftingMenu.recipeListIcon;
            _recipeListName = craftingMenu.recipeListName;
            _recipeRegistry = craftingMenu.recipeRegistry;
            _itemRegistry = craftingMenu.itemRegistry;
            _recipeListing = craftingMenu.recipeListing;
            _currentRecipeIndex = -1;
            _recipeListContainer = craftingMenu.recipeListContainer;
            _recipeList = null;
        }

        public override void Open()
        {
            _recipeListIcon.style.backgroundImage = new StyleBackground(_itemTypeIcons[_itemType]);
            _recipeListName.text = _itemType.ToString();

            List<CraftingRecipe> recipes = _recipeRegistry.GetOfType(_itemType, _machineType).ToList();

            VisualElement MakeItem()
            {
                VisualElement recipe = _recipeListing.Instantiate();
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


                if (recipes[idx].CanCraft(_machineType, _craftingMenu.Inventory.GetItems()))
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

            // Fix slow mouse wheel scrolling

            Scroller scroller = recipeList.Q<Scroller>();
            recipeList.RegisterCallback<WheelEvent>(evt =>
            {
                scroller.value += evt.delta.y * 100;
                evt.StopPropagation();
            });


            _recipeList = recipeList;
            _recipeListContainer.Add(recipeList);
        }

        private void OpenRecipeView(CraftingRecipe recipe, int index)
        {
            int oldRecipeIndex = _currentRecipeIndex;
            _currentRecipeIndex = index;
            if (oldRecipeIndex != -1)
            {
                _recipeList.RefreshItem(oldRecipeIndex);
            }

            _recipeList.RefreshItem(_currentRecipeIndex);

            _recipeView?.Close();
            _recipeView = new RecipeView(_craftingMenu, recipe);
            _recipeView.Open();
        }


        public override void Close()
        {
            _recipeView?.Close();
        }

        public override void Refresh()
        {
            _recipeList.RefreshItems();
            _recipeView?.Refresh();
        }
    }
}