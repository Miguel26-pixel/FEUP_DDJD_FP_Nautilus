using Inventory;
using Items;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Inventory
{
    public class PlayerInventoryViewer : InventoryViewer<PlayerInventory>
    {
        private readonly PlayerInventory _inventory;

        public PlayerInventoryViewer(VisualElement inventoryContainer, PlayerInventory inventory) : base(
            inventoryContainer, inventory)
        {
            _inventory = inventory;
        }

        public override void Show()
        {
            inventoryContainer.Clear();
            for (int row = 0; row < InventoryConstants.PlayerInventoryMaxHeight; row++)
            {
                VisualElement rowElement = new();
                rowElement.AddToClassList("row");
                rowElement.name = $"Row{row}";

                for (int col = 0; col < InventoryConstants.PlayerInventoryMaxWidth; col++)
                {
                    VisualElement cell = new();
                    cell.name = $"Cell{col}";
                    cell.AddToClassList("item-square");

                    VisualElement iconElement = new();
                    iconElement.name = "ItemIcon";
                    iconElement.AddToClassList("icon");

                    VisualElement overlayElement = new();
                    overlayElement.name = "Overlay";
                    overlayElement.AddToClassList("overlay");

                    cell.Add(iconElement);
                    cell.Add(overlayElement);

                    if (!_inventory.ValidatePosition(new Vector2Int(col, row)))
                    {
                        cell.visible = false;
                    }

                    // Get sprite from item, and rotate it if necessary
                    ItemPositionAndID itemPositionAndID = _inventory.GetItemPositionAt(new Vector2Int(col, row));

                    if (itemPositionAndID != null)
                    {
                        Vector2Int relativePosition = itemPositionAndID.relativePosition;
                        int rotation = itemPositionAndID.rotation;
                        Item item = _inventory.GetAt(new Vector2Int(col, row));
                        Sprite[,] rotatedIcons = ItemGrid<Sprite>.RotateMultiple(item.Icons, rotation);
                        
                        Sprite icon = rotatedIcons[relativePosition.y, relativePosition.x];
                        
                        iconElement.style.rotate = new StyleRotate(new Rotate(new Angle(-90 * rotation)));
                        iconElement.style.backgroundImage = new StyleBackground(icon);
                    }

                    rowElement.Add(cell);
                }

                inventoryContainer.Add(rowElement);
            }
        }
    }
}