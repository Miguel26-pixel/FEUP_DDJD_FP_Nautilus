using System;
using Inventory;
using UnityEngine.UIElements;

namespace UI.Inventory
{
    public abstract class InventoryViewerBuilder<T> where T : IInventory
    {
        public VisualElement root;
        public VisualElement inventoryContainer;
        public VisualTreeAsset itemDescriptorTemplate;
        public Action<IDraggable> onDragStart = null;
        public Action<IDraggable> onDragEnd = null;
        public readonly T inventory;
        protected readonly bool canMove;
        protected readonly bool canOpenContextMenu;
        protected readonly bool refreshAfterMove;

        protected InventoryViewerBuilder(T inventory, bool canMove = true, bool canOpenContextMenu = true, bool refreshAfterMove = true)
        {
            this.inventory = inventory;
            this.canMove = canMove;
            this.canOpenContextMenu = canOpenContextMenu;
            this.refreshAfterMove = refreshAfterMove;
        }

        public abstract InventoryViewer<T> Build();
    }
}