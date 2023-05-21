using Inventory;
using UI.Inventory.Components;

namespace UI.Inventory.Builders
{
    public class GridInventoryViewerBuilder : InventoryViewerBuilder<InventoryGrid>
    {
        public GridInventoryViewerBuilder(InventoryGrid inventory, bool canMove = true, bool canOpenContextMenu = true,
            bool refreshAfterMove = true) : base(inventory, canMove, canOpenContextMenu, refreshAfterMove)
        {
        }

        public override InventoryViewer<InventoryGrid> Build()
        {
            return new GridInventoryViewer(root, inventoryContainer, inventory, canMove: canMove,
                canOpenContext: canOpenContextMenu, refreshAfterMove: refreshAfterMove);
        }
    }
}