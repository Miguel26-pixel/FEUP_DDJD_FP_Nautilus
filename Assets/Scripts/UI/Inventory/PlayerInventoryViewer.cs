using Inventory;
using UnityEngine.UIElements;

namespace UI.Inventory
{
    public class PlayerInventoryViewer : GridInventoryViewer<PlayerInventory>, IInventorySubscriber

    {
        public PlayerInventoryViewer(VisualElement root, VisualElement inventoryContainer,
            VisualTreeAsset itemDescriptorTemplate, PlayerInventory inventory) : base(root, inventoryContainer,
            itemDescriptorTemplate, inventory, refreshAfterMove: false)
        {
            inventory.AddSubscriber(this);
        }

        public void OnInventoryChanged()
        {
            Refresh();
        }

        public override void Close()
        {
            base.Close();
            inventory.RemoveSubscriber(this);
        }
    }
}