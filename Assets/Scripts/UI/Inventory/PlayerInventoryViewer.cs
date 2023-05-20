using System;
using Inventory;
using Items;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace UI.Inventory
{
    public class PlayerInventoryViewer : InventoryViewer<PlayerInventory>
    {
        private readonly PlayerInventory _inventory;
        private readonly BoundsInt _inventoryBounds;
        private readonly VisualElement[,] _inventoryCells = new VisualElement[InventoryConstants.PlayerInventoryMaxHeight,
            InventoryConstants.PlayerInventoryMaxWidth];

        public PlayerInventoryViewer(VisualElement root, VisualElement inventoryContainer, PlayerInventory inventory) : base(
            root, inventoryContainer, inventory)
        {
            _inventory = inventory;
            _inventoryBounds = _inventory.GetBounds();
        }

        public override void Show()
        {
            inventoryContainer.Clear();
            Array.Clear(_inventoryCells, 0, _inventoryCells.Length);
            
            // root.RegisterCallback<MouseUpEvent>(e => ProcessMouseUp(e, "root"));

            for (int row = _inventoryBounds.y; row < _inventoryBounds.y + _inventoryBounds.size.y; row++)
            {
                VisualElement rowElement = new();
                rowElement.AddToClassList("row");
                rowElement.name = $"Row{row}";
                uint previousID = 0;

                for (int col = _inventoryBounds.x; col < _inventoryBounds.x + _inventoryBounds.size.x; col++)
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

                    VisualElement background = new();
                    background.name = "Background";
                    background.AddToClassList("cell-background");

                    cell.Add(background);
                    cell.Add(iconElement);
                    cell.Add(overlayElement);
                    
                    _inventoryCells[row, col] = cell;

                    Vector2Int position = new Vector2Int(col, row);
                    if (!_inventory.ValidatePosition(position))
                    {
                        cell.visible = false;
                    }

                    // Get sprite from item, and rotate it if necessary
                    RelativePositionAndID relativePositionAndID = _inventory.GetItemPositionAt(new Vector2Int(col, row));

                    if (relativePositionAndID != null)
                    {
                        RenderIconSquare(relativePositionAndID, background, iconElement, new Vector2Int(col, row), previousID);
                        cell.RegisterCallback<MouseDownEvent>(evt => ProcessCellClick(evt, cell, position));
                        previousID = relativePositionAndID.itemID;
                    }
                    else
                    {
                        previousID = 0;
                        // cell.RegisterCallback<MouseUpEvent>(e => ProcessMouseUp(e, "cell"));
                    }

                    rowElement.Add(cell);
                }

                inventoryContainer.Add(rowElement);
            }
        }

        private void ProcessCellClick(MouseDownEvent evt, VisualElement cell, Vector2Int position)
        {
            Item item = _inventory.GetAt(position);
            if (item == null)
            {
                return;
            }
            
            DarkenItem(_inventory.GetItemPositionAt(position));
        }

        private void DarkenItem(RelativePositionAndID positionAndID)
        {
            Vector2Int initialPosition = _inventory.GetInitialPosition(positionAndID.itemID);
            
            for (int y = initialPosition.y; y < initialPosition.y + ItemConstants.ItemWidth; y++)
            {
                for (int x = initialPosition.x; x < initialPosition.x + ItemConstants.ItemHeight; x++)
                {
                    if (!_inventory.ValidatePosition(new Vector2Int(x, y)))
                    {
                        continue;
                    }
                    
                    VisualElement cell = _inventoryCells[y, x];
                    RelativePositionAndID relativePositionAndID = _inventory.GetItemPositionAt(new Vector2Int(x, y));

                    if (relativePositionAndID != null && relativePositionAndID.itemID == positionAndID.itemID)
                    {
                        cell.AddToClassList("disabled");
                    }
                }
            }
        }

        private void ProcessMouseUpRoot(MouseUpEvent evt)
        {
        }

        private void ProcessMouseUpCell(MouseUpEvent evt)
        {
            
        }

        private void RenderIconSquare(RelativePositionAndID relativePositionAndID, VisualElement cell, VisualElement iconElement, Vector2Int position, uint previousID)
        {
            Vector2Int relativePosition = relativePositionAndID.relativePosition;
            int rotation = relativePositionAndID.rotation;
            Item item = _inventory.GetAt(position);
            Sprite[,] rotatedIcons = ItemGrid<Sprite>.RotateMultiple(item.Icons, rotation);
                        
            Sprite icon = rotatedIcons[relativePosition.y, relativePosition.x];
                        
            iconElement.style.rotate = new StyleRotate(new Rotate(new Angle(-90 * rotation)));
            iconElement.style.backgroundImage = new StyleBackground(icon);
            
            // Render a border over the same item in the 4 directions
            
            // Top
            if (CheckBorder(position + Vector2Int.down, relativePositionAndID.itemID))
            {
                cell.style.borderTopWidth = new StyleFloat { value = 0 };
            }
            // Bottom
            if (CheckBorder(position + Vector2Int.up, relativePositionAndID.itemID))
            {
                cell.style.borderBottomWidth = new StyleFloat { value = 0 };
            }
            // Left
            if (previousID == relativePositionAndID.itemID)
            {
                cell.style.borderLeftWidth = new StyleFloat { value = 0 };
            }
            // Right
            if (CheckBorder(position + Vector2Int.right, relativePositionAndID.itemID))
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
            
            RelativePositionAndID relativePositionAndID = _inventory.GetItemPositionAt(position);
            return relativePositionAndID != null && relativePositionAndID.itemID == itemID;
        }
    }
}