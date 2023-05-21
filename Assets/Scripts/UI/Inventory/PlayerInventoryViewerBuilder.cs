using Inventory;

namespace UI.Inventory
{
    public class PlayerInventoryViewerBuilder : GridInventoryViewerBuilder
    {
        private readonly PlayerInventory _inventory;
        
        public PlayerInventoryViewerBuilder(PlayerInventory inventory, bool canMove = true, bool canOpenContextMenu = true, bool refreshAfterMove = true) : base(inventory, canMove, canOpenContextMenu, refreshAfterMove)
        {
            _inventory = inventory;
        }

        public override InventoryViewer<InventoryGrid> Build()
        {
            return new PlayerInventoryViewer(root, inventoryContainer, itemDescriptorTemplate, _inventory);
        }
    }
}