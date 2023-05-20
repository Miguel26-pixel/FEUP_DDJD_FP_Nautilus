using Inventory;
using UnityEngine.UIElements;

namespace UI.Inventory
{
    public abstract class InventoryViewer<T> where T : IInventory
    {
        protected readonly VisualElement inventoryContainer;
        protected readonly VisualElement root;
        protected readonly VisualTreeAsset itemDescriptorTemplate;
        protected IInventory inventory;

        protected InventoryViewer(VisualElement root, VisualElement inventoryContainer, VisualTreeAsset itemDescriptorTemplate, T inventory)
        {
            this.root = root;
            this.inventoryContainer = inventoryContainer;
            this.inventory = inventory;
            this.itemDescriptorTemplate = itemDescriptorTemplate;
        }

        public abstract void Show();

        public abstract void Update();

        public abstract void Refresh();
        
        public abstract void Rotate(int direction);
    }
}