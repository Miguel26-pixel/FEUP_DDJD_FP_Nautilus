using Inventory;
using UnityEngine.UIElements;

namespace UI.Inventory.Components
{
    public class PlayerInventoryViewer : GridInventoryViewer, IInventorySubscriber

    {
        private readonly PlayerInventory _inventory;

        public PlayerInventoryViewer(VisualElement root, VisualElement inventoryContainer, PlayerInventory inventory) :
            base(root, inventoryContainer, inventory, refreshAfterMove: false)
        {
            inventory.AddSubscriber(this);
            _inventory = inventory;
        }

        public void OnInventoryChanged()
        {
            Refresh();
        }

        public override void Close()
        {
            base.Close();
            _inventory.RemoveSubscriber(this);
        }
    }
}