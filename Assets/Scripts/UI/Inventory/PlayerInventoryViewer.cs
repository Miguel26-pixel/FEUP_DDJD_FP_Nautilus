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
        
        private Vector2Int _dragStartPosition;
        private bool _isDragging;
        private readonly VisualElement _draggedItem;
        private readonly float _cellWidth;
        private readonly float _cellHeight;

            public PlayerInventoryViewer(VisualElement root, VisualElement inventoryContainer, PlayerInventory inventory) : base(
            root, inventoryContainer, inventory)
        {
            _inventory = inventory;
            _inventoryBounds = _inventory.GetBounds();
            _draggedItem = root.Q<VisualElement>("ItemDrag");
            _draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            
            VisualElement cell = inventoryContainer.Q<VisualElement>("Cell0");
            _cellWidth = cell.style.width.value.value;
            _cellHeight = cell.style.height.value.value;
            
            Debug.Log($"Cell width: {_cellWidth}, Cell height: {_cellHeight}");
        }

        public override void Show()
        {
            inventoryContainer.Clear();
            Array.Clear(_inventoryCells, 0, _inventoryCells.Length);

            root.RegisterCallback<MouseUpEvent>(ProcessMouseUpRoot);

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
                        previousID = 0;
                        rowElement.Add(cell);
                        
                        continue;
                    }

                    // Get sprite from item, and rotate it if necessary
                    RelativePositionAndID relativePositionAndID = _inventory.GetItemPositionAt(position);

                    if (relativePositionAndID != null)
                    {
                        RenderIconSquare(relativePositionAndID, iconElement, _inventory.GetAt(position));
                        MergeBorders(relativePositionAndID, background, position, previousID);
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

        public override void Update()
        {
            if (!_isDragging)
            {
                return;
            }

            Vector2 mousePos = Mouse.current.position.ReadValue();
            mousePos = RuntimePanelUtils.ScreenToPanel(root.panel, mousePos);
            
            _draggedItem.style.left = mousePos.x - 26;
            _draggedItem.style.top = root.resolvedStyle.height - mousePos.y - 26;
        }

        public override void Refresh()
        {
            throw new NotImplementedException();
        }

        private void ProcessCellClick(MouseDownEvent evt, VisualElement cell, Vector2Int position)
        {
            Item item = _inventory.GetAt(position);
            if (item == null)
            {
                return;
            }

            _isDragging = true;
            _dragStartPosition = position;
            uint itemID = _inventory.GetItemPositionAt(position).itemID;
            
            DarkenItem(itemID);
            RenderItemDrag(itemID);
        }
        
        private void ProcessMouseUpRoot(MouseUpEvent evt)
        {
            if (!_isDragging)
            {
                return;
            }
            
            _isDragging = false;
            _draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            DarkenItem(_inventory.GetItemPositionAt(_dragStartPosition).itemID, false);
        }

        private void RenderItemDrag(uint itemID)
        {
            _draggedItem.Clear();
            Vector2Int initialPosition = _inventory.GetInitialPosition(itemID);

            for (int y = initialPosition.y; y < initialPosition.y + ItemConstants.ItemHeight; y++)
            {
                VisualElement row = new();
                row.AddToClassList("row");

                for (int x = initialPosition.x; x < initialPosition.x + ItemConstants.ItemWidth; x++)
                {
                    VisualElement cell = new();
                    cell.AddToClassList("item-square");
                    
                    VisualElement iconElement = new();
                    iconElement.name = "ItemIcon";
                    iconElement.AddToClassList("icon");

                    cell.Add(iconElement);
                    
                    Vector2Int position = new(x, y);
                    if (!_inventory.ValidatePosition(position))
                    {
                        cell.visible = false;
                        row.Add(cell);

                        continue;
                    }
                    
                    RelativePositionAndID relativePositionAndID = _inventory.GetItemPositionAt(position);
                    if (relativePositionAndID != null && relativePositionAndID.itemID == itemID)
                    {
                        RenderIconSquare(relativePositionAndID, iconElement, _inventory.GetAt(position));
                    }
                    else
                    {
                        cell.visible = false;
                    }
                    
                    row.Add(cell);
                }
                
                _draggedItem.Add(row);
            }

            _draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        }

        private void DarkenItem(uint itemID, bool darken = true)
        {
            Vector2Int initialPosition = _inventory.GetInitialPosition(itemID);

            for (int y = initialPosition.y; y < initialPosition.y + ItemConstants.ItemHeight; y++)
            {
                for (int x = initialPosition.x; x < initialPosition.x + ItemConstants.ItemWidth; x++)
                {
                    if (!_inventory.ValidatePosition(new Vector2Int(x, y)))
                    {
                        continue;
                    }

                    VisualElement cell = _inventoryCells[y, x];
                    RelativePositionAndID relativePositionAndID = _inventory.GetItemPositionAt(new Vector2Int(x, y));

                    if (relativePositionAndID == null || relativePositionAndID.itemID != itemID)
                    {
                        continue;
                    }

                    if (darken)
                    {
                        cell.AddToClassList("disabled");
                    }
                    else
                    {
                        cell.RemoveFromClassList("disabled");
                    }
                }
            }
        }

        private void ProcessMouseUpCell(MouseUpEvent evt)
        {
            
        }


        private void MergeBorders(RelativePositionAndID relativePositionAndID, VisualElement cell, Vector2Int position, uint previousID)
        {
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

        private void RenderIconSquare(RelativePositionAndID relativePositionAndID, VisualElement iconElement, Item item)
        {
            Vector2Int relativePosition = relativePositionAndID.relativePosition;
            int rotation = relativePositionAndID.rotation;
            // TODO: This rotation should not really be done on every square, but don't know how to do it otherwise
            Sprite[,] rotatedIcons = ItemGrid<Sprite>.RotateMultiple(item.Icons, rotation);
                        
            Sprite icon = rotatedIcons[relativePosition.y, relativePosition.x];
                        
            iconElement.style.rotate = new StyleRotate(new Rotate(new Angle(-90 * rotation)));
            iconElement.style.backgroundImage = new StyleBackground(icon);
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