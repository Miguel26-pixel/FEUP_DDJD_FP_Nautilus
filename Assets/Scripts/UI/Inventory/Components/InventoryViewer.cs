using System;
using Inventory;
using Items;
using UnityEngine.UIElements;

namespace UI.Inventory.Components
{
    public interface IDraggable
    {
        Item Item { get; }
    }


    public abstract class InventoryViewer<T> where T : IInventory
    {
        protected readonly bool canMove;
        protected readonly bool canOpenContextMenu;
        protected readonly T inventory;
        protected readonly VisualElement inventoryContainer;
        protected readonly bool refreshAfterMove;
        protected readonly VisualElement root;
        public Action<IDraggable> onDragEnd;

        public Action<IDraggable> onDragStart;

        protected InventoryViewer(VisualElement root, VisualElement inventoryContainer,
            T inventory, Action<IDraggable> onDragStart = null, Action<IDraggable> onDragEnd = null,
            bool canMove = true, bool canOpenContextMenu = true, bool refreshAfterMove = true)
        {
            this.root = root;
            this.inventoryContainer = inventoryContainer;
            this.inventory = inventory;
            this.canMove = canMove;
            this.canOpenContextMenu = canOpenContextMenu;
            this.refreshAfterMove = refreshAfterMove;
            this.onDragStart = onDragStart;
            this.onDragEnd = onDragEnd;
        }

        public abstract void Show();

        public abstract void Close();

        public abstract void Update();

        public abstract void Refresh();

        public abstract void Rotate(int direction);

        public abstract void HandleDragStart(IDraggable draggable);

        public abstract void HandleDragEnd(IDraggable draggable);
    }
}