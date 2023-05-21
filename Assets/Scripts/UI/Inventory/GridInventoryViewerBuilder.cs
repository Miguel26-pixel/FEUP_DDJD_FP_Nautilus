using Inventory;

namespace UI.Inventory
{
    public class GridInventoryViewerBuilder: InventoryViewerBuilder<InventoryGrid>
    {
        public GridInventoryViewerBuilder(InventoryGrid inventory, bool canMove = true, bool canOpenContextMenu = true, bool refreshAfterMove = true) : base(inventory, canMove, canOpenContextMenu, refreshAfterMove)
        {
        }

        public override InventoryViewer<InventoryGrid> Build()
        {
            return new GridInventoryViewer(root, inventoryContainer, itemDescriptorTemplate, inventory, onDragStart, onDragEnd, canMove, canOpenContextMenu, refreshAfterMove);
        }
    }
}