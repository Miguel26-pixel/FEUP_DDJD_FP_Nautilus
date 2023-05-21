using System;
using Inventory;
using Items;
using UnityEngine.UIElements;

namespace UI.Inventory
{
    public interface IDraggable
    {
        Item Item { get; }
    }
    
    
    public abstract class InventoryViewer<T> where T : IInventory
    {
        protected readonly VisualElement inventoryContainer;
        protected readonly VisualTreeAsset itemDescriptorTemplate;
        protected readonly VisualElement root;
        protected readonly T inventory;
        protected readonly bool canMove;
        protected readonly bool canOpenContextMenu;
        protected readonly bool refreshAfterMove;
        
        public Action<IDraggable> onDragStart;
        public Action<IDraggable> onDragEnd;

        protected InventoryViewer(VisualElement root, VisualElement inventoryContainer,
            VisualTreeAsset itemDescriptorTemplate, T inventory, Action<IDraggable> onDragStart = null, Action<IDraggable> onDragEnd = null ,bool canMove = true, bool canOpenContextMenu = true, bool refreshAfterMove = true)
        {
            this.root = root;
            this.inventoryContainer = inventoryContainer;
            this.inventory = inventory;
            this.itemDescriptorTemplate = itemDescriptorTemplate;
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