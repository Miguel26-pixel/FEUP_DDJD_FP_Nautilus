using System;
using Inventory;
using Items;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace UI.Inventory
{
    public record DraggingProperties()
    {
        public Vector2Int dragStartPosition;
        public Vector2Int dragRelativePosition;
        public Item draggedItem;
        public int draggedRotation;

        public DraggingProperties(Vector2Int dragStartPosition, Vector2Int dragRelativePosition, Item draggedItem, int draggedRotation) : this()
        {
            this.dragStartPosition = dragStartPosition;
            this.dragRelativePosition = dragRelativePosition;
            this.draggedItem = draggedItem;
            this.draggedRotation = draggedRotation;
        }
    }
    
    public class PlayerInventoryViewer : InventoryViewer<PlayerInventory>
    {
        private readonly PlayerInventory _inventory;
        private readonly BoundsInt _inventoryBounds;
        private readonly VisualElement[,] _inventoryCells = new VisualElement[InventoryConstants.PlayerInventoryMaxHeight,
            InventoryConstants.PlayerInventoryMaxWidth];
        
        private bool _isDragging;
        private DraggingProperties _draggingProperties;
        private readonly VisualElement _draggedItem;
        private float _cellWidth;
        private float _cellHeight;

            public PlayerInventoryViewer(VisualElement root, VisualElement inventoryContainer, PlayerInventory inventory) : base(
            root, inventoryContainer, inventory)
        {
            _inventory = inventory;
            _inventoryBounds = _inventory.GetBounds();
            _draggedItem = root.Q<VisualElement>("ItemDrag");
            _draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }

        public override void Show()
        {
            inventoryContainer.Clear();
            Array.Clear(_inventoryCells, 0, _inventoryCells.Length);
            bool registeredGeometryChange = false;

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
                        RenderIconSquare(relativePositionAndID.relativePosition, relativePositionAndID.rotation, iconElement, _inventory.GetAt(position));
                        MergeBorders(relativePositionAndID, background, position, previousID);
                        cell.RegisterCallback<MouseDownEvent>(evt => ProcessCellClick(evt, cell, position));
                        previousID = relativePositionAndID.itemID;
                    }
                    else
                    {
                        previousID = 0;
                        // cell.RegisterCallback<MouseUpEvent>(e => ProcessMouseUp(e, "cell"));
                    }

                    if (!registeredGeometryChange)
                    {
                        cell.RegisterCallback<GeometryChangedEvent>(evt =>
                        {
                            _cellWidth = cell.resolvedStyle.width;
                            _cellHeight = cell.resolvedStyle.height;
                        });
                        registeredGeometryChange = true;
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
            
            _draggedItem.style.left = mousePos.x - (_draggingProperties.dragRelativePosition.x + 0.33f) * _cellWidth;
            _draggedItem.style.top = root.resolvedStyle.height - mousePos.y - (_draggingProperties.dragRelativePosition.y + 0.33f) * _cellHeight;
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
            RelativePositionAndID relativePositionAndID = _inventory.GetItemPositionAt(position);
            _draggingProperties = new DraggingProperties(position, relativePositionAndID.relativePosition, item, relativePositionAndID.rotation);
            uint itemID = _inventory.GetItemPositionAt(position).itemID;
            
            DarkenItem(itemID);
            RenderItemDrag();
        }
        
        private void ProcessMouseUpRoot(MouseUpEvent evt)
        {
            if (!_isDragging)
            {
                return;
            }
            
            _isDragging = false;
            _draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            DarkenItem(_inventory.GetItemPositionAt(_draggingProperties.dragStartPosition).itemID, false);
        }

        private void RenderItemDrag()
        {
            _draggedItem.Clear();
            
            bool[,] itemGrid = ItemGrid<bool>.RotateMultiple(_draggingProperties.draggedItem.Grid, _draggingProperties.draggedRotation);
            BoundsInt bounds = ItemGrid<bool>.GetBounds(itemGrid, true);

            for (int y = bounds.y; y < bounds.y + ItemConstants.ItemHeight; y++)
            {
                VisualElement row = new();
                row.AddToClassList("row");

                for (int x = bounds.x; x < bounds.x + ItemConstants.ItemWidth; x++)
                {
                    VisualElement cell = new();
                    cell.AddToClassList("item-square");
                    
                    VisualElement iconElement = new();
                    iconElement.name = "ItemIcon";
                    iconElement.AddToClassList("icon");

                    cell.Add(iconElement);
                    
                    if (itemGrid[y, x])
                    {
                        RenderIconSquare(new Vector2Int(x, y), _draggingProperties.draggedRotation, iconElement, _draggingProperties.draggedItem);
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

        private void RenderIconSquare(Vector2Int relativePosition, int rotation, VisualElement iconElement, Item item)
        {
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