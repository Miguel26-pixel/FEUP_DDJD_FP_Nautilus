using Inventory;
using PlayerControls;
using UI.Inventory.Components;

namespace UI.Inventory.Builders
{
    public class GridInventoryViewerBuilder : InventoryViewerBuilder<InventoryGrid>
    {
        public GridInventoryViewerBuilder(InventoryGrid inventory, Player player, bool canMove = true, bool canOpenContextMenu = true,
            bool refreshAfterMove = true) : base(inventory, player, canMove, canOpenContextMenu, refreshAfterMove)
        {
        }

        public override InventoryViewer<InventoryGrid> Build()
        {
            return new GridInventoryViewer(root, inventoryContainer, inventory, player, canMove: canMove,
                canOpenContext: canOpenContextMenu, refreshAfterMove: refreshAfterMove);
        }
    }
}