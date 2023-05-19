using Inventory;
using UnityEngine.UIElements;

namespace UI.Inventory
{
    public abstract class InventoryViewer<T> where T : IInventory
    {
        protected readonly VisualElement inventoryContainer;
        protected IInventory inventory;

        protected InventoryViewer(VisualElement inventoryContainer, T inventory)
        {
            this.inventoryContainer = inventoryContainer;
            this.inventory = inventory;
        }

        public abstract void Show();
    }
}