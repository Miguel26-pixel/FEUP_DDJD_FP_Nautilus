using Inventory;
using UnityEngine.UIElements;

namespace UI.Inventory
{
    public abstract class InventoryViewer<T> where T : IInventory
    {
        protected readonly VisualElement inventoryContainer;
        protected readonly VisualTreeAsset itemDescriptorTemplate;
        protected readonly VisualElement root;
        protected T inventory;
        protected bool canMove;
        protected bool canOpenContextMenu;
        protected bool refreshAfterMove;

        protected InventoryViewer(VisualElement root, VisualElement inventoryContainer,
            VisualTreeAsset itemDescriptorTemplate, T inventory, bool canMove = true, bool canOpenContextMenu = true, bool refreshAfterMove = true)
        {
            this.root = root;
            this.inventoryContainer = inventoryContainer;
            this.inventory = inventory;
            this.itemDescriptorTemplate = itemDescriptorTemplate;
            this.canMove = canMove;
            this.canOpenContextMenu = canOpenContextMenu;
            this.refreshAfterMove = refreshAfterMove;
        }

        public abstract void Show();

        public abstract void Close();

        public abstract void Update();

        public abstract void Refresh();

        public abstract void Rotate(int direction);
    }
}