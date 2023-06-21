using Inventory;
using PlayerControls;
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
        protected readonly Player player;

        protected InventoryViewerBuilder(T inventory, Player player, bool canMove = true, bool canOpenContextMenu = true,
            bool refreshAfterMove = true)
        {
            this.inventory = inventory;
            this.player = player;
            this.canMove = canMove;
            this.canOpenContextMenu = canOpenContextMenu;
            this.refreshAfterMove = refreshAfterMove;
        }

        public abstract InventoryViewer<T> Build();
    }
}