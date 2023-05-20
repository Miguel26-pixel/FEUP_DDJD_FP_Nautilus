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
            
            // Get inventory bounds
            BoundsInt bounds = _inventory.GetBounds();

            for (int row = bounds.y; row < bounds.y + bounds.size.y; row++)
            {
                VisualElement rowElement = new();
                rowElement.AddToClassList("row");
                rowElement.name = $"Row{row}";
                uint previousID = 0;

                for (int col = bounds.x; col < bounds.x + bounds.size.x; col++)
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
                        RenderIconSquare(itemPositionAndID, cell, iconElement, new Vector2Int(col, row), previousID);
                        previousID = itemPositionAndID.itemID;
                    }
                    else
                    {
                        previousID = 0;
                    }

                    rowElement.Add(cell);
                }

                inventoryContainer.Add(rowElement);
            }
        }

        private void RenderIconSquare(ItemPositionAndID itemPositionAndID, VisualElement cell, VisualElement iconElement, Vector2Int position, uint previousID)
        {
            Vector2Int relativePosition = itemPositionAndID.relativePosition;
            int rotation = itemPositionAndID.rotation;
            Item item = _inventory.GetAt(position);
            Sprite[,] rotatedIcons = ItemGrid<Sprite>.RotateMultiple(item.Icons, rotation);
                        
            Sprite icon = rotatedIcons[relativePosition.y, relativePosition.x];
                        
            iconElement.style.rotate = new StyleRotate(new Rotate(new Angle(-90 * rotation)));
            iconElement.style.backgroundImage = new StyleBackground(icon);
            
            // Render a border over the same item in the 4 directions
            
            // Top
            if (CheckBorder(position + Vector2Int.down, itemPositionAndID.itemID))
            {
                cell.style.borderTopWidth = new StyleFloat { value = 0 };
            }
            // Bottom
            if (CheckBorder(position + Vector2Int.up, itemPositionAndID.itemID))
            {
                cell.style.borderBottomWidth = new StyleFloat { value = 0 };
            }
            // Left
            if (previousID == itemPositionAndID.itemID)
            {
                cell.style.borderLeftWidth = new StyleFloat { value = 0 };
            }
            // Right
            if (CheckBorder(position + Vector2Int.right, itemPositionAndID.itemID))
            {
                cell.style.borderRightWidth = new StyleFloat { value = 0 };
            }
        }

        private bool CheckBorder(Vector2Int position, uint itemID)
        {
            if (!_inventory.ValidatePosition(position))
            {
                return false;
            }
            
            ItemPositionAndID itemPositionAndID = _inventory.GetItemPositionAt(position);
            return itemPositionAndID != null && itemPositionAndID.itemID == itemID;
        }
    }
}