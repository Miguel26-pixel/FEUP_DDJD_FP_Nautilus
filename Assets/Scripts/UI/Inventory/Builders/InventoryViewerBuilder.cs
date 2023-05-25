using Inventory;
using UI.Inventory.Components;
using UnityEngine.UIElements;

namespace UI.Inventory.Builders
{
    public abstract class InventoryViewerBuilder<T> where T : IInventory
    {
        protected readonly bool canMove;
        protected readonly bool canOpenContextMenu;
        public readonly T inventory;
        protected readonly bool refreshAfterMove;
        public VisualElement inventoryContainer;
        public VisualElement root;

        protected InventoryViewerBuilder(T inventory, bool canMove = true, bool canOpenContextMenu = true,
            bool refreshAfterMove = true)
        {
            this.inventory = inventory;
            this.canMove = canMove;
            this.canOpenContextMenu = canOpenContextMenu;
            this.refreshAfterMove = refreshAfterMove;
        }

        public abstract InventoryViewer<T> Build();
    }
}