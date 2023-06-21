using Inventory;
using PlayerControls;
using UI.Inventory.Components;

namespace UI.Inventory.Builders
{
    public class PlayerInventoryViewerBuilder : GridInventoryViewerBuilder
    {
        private readonly PlayerInventory _inventory;
        private readonly Player _player;

        public PlayerInventoryViewerBuilder(PlayerInventory inventory, Player player, bool canMove = true,
            bool canOpenContextMenu = true, bool refreshAfterMove = true) : base(inventory, player, canMove, canOpenContextMenu,
            refreshAfterMove)
        {
            _inventory = inventory;
            _player = player;
        }

        public override InventoryViewer<InventoryGrid> Build()
        {
            return new PlayerInventoryViewer(root, inventoryContainer, _inventory, _player);
        }
    }
}