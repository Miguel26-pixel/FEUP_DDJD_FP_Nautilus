using Inventory;
using UnityEngine.UIElements;

namespace UI.Inventory
{
    public abstract class InventoryViewer<T> where T : IInventory
    {
        protected readonly VisualElement root;
        protected readonly VisualElement inventoryContainer;
        protected IInventory inventory;

        protected InventoryViewer(VisualElement root, VisualElement inventoryContainer, T inventory)
        {
            this.root = root;
            this.inventoryContainer = inventoryContainer;
            this.inventory = inventory;
        }

        public abstract void Show();

        public abstract void Update();

        public abstract void Refresh();
    }
}