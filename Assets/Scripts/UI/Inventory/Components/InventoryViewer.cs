using System;
using Inventory;
using Items;
using PlayerControls;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Inventory.Components
{
    public interface IDraggable
    {
        Item Item { get; }
    }
    
    public record ItemDraggable() : DraggingProperties
    {
        public ItemDraggable(Item item, EquipmentSlotType equipmentSlotType) : this()
        {
            dragStartPosition = new ItemPosition(Vector2Int.zero, 0);
            dragRelativePosition = Vector2Int.zero;
            draggedItem = item;
            currentRotation = dragStartPosition.rotation;
            itemID = 0;
            Item = item;
            EquipmentSlotType = equipmentSlotType;
        }

        public Item Item { get; }
        public EquipmentSlotType EquipmentSlotType { get; }
    }


    public abstract class InventoryViewer<T> where T : IInventory
    {
        protected readonly bool canMove;
        protected readonly bool canOpenContextMenu;
        protected readonly T inventory;
        protected readonly Player player;
        protected readonly VisualElement inventoryContainer;
        protected readonly bool refreshAfterMove;
        protected readonly VisualElement root;
        public Action<IDraggable> onDragEnd;

        public Action<IDraggable> onDragStart;

        protected InventoryViewer(VisualElement root, VisualElement inventoryContainer,
            T inventory, Player player, Action<IDraggable> onDragStart = null, Action<IDraggable> onDragEnd = null,
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
            this.player = player;
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