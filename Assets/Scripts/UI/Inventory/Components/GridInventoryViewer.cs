using System;
using System.Collections.Generic;
using Inventory;
using Items;
using UI.Inventory.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace UI.Inventory
{
    public record DraggingProperties() : IDraggable
    {
        public readonly Item draggedItem;
        public readonly ItemPosition dragStartPosition;
        public readonly uint itemID;
        public int currentRotation;
        public Vector2Int dragRelativePosition;

        public DraggingProperties(ItemPosition dragStartPosition, Vector2Int dragRelativePosition, Item draggedItem,
            uint itemID) : this()
        {
            this.dragStartPosition = dragStartPosition;
            this.dragRelativePosition = dragRelativePosition;
            this.draggedItem = draggedItem;
            currentRotation = this.dragStartPosition.rotation;
            this.itemID = itemID;
        }

        public Item Item => draggedItem;
    }

    public class GridInventoryViewer : InventoryViewer<InventoryGrid>
    {
        private readonly InventoryGrid _inventory;
        private readonly BoundsInt _inventoryBounds;

        private readonly VisualElement[,] _inventoryCells =
            new VisualElement[InventoryConstants.PlayerInventoryMaxHeight,
                InventoryConstants.PlayerInventoryMaxWidth];

        private float _cellHeight;
        private float _cellWidth;
        
        private readonly InfoBoxViewer _infoBoxViewer;
        private readonly ContextMenuViewer _contextMenuViewer;

        private readonly VisualElement _draggedItem;
        private DraggingProperties _draggingProperties;
        private Vector2 _currentMousePosition;
        private bool _isDragging;

        private IDraggable _otherDraggable;

        private bool _registeredGeometryChange;

        public GridInventoryViewer(VisualElement root, VisualElement inventoryContainer,
            VisualTreeAsset itemDescriptorTemplate, InventoryGrid inventory, Action<IDraggable> onDragStart = null, Action<IDraggable> onDragEnd = null,  bool canMove = true, bool canOpenContext = true, bool refreshAfterMove = true
            ) :
            base(
                root, inventoryContainer, itemDescriptorTemplate, inventory, onDragStart, onDragEnd, canMove, canOpenContext, refreshAfterMove)
        {
            _inventory = inventory;
            _inventoryBounds = _inventory.GetBounds();
            _draggedItem = root.Q<VisualElement>("ItemDrag");
            _draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            
            _infoBoxViewer = new InfoBoxViewer(root, root.Q<VisualElement>("ItemInfo"));
            _contextMenuViewer = new ContextMenuViewer(root, root.Q<VisualElement>("ItemContext"));
        }

        public override void Show()
        {
            _registeredGeometryChange = false;
            root.RegisterCallback<MouseUpEvent>(ProcessMouseUpRoot);

            Refresh();
        }

        public override void Close()
        {
        }

        public override void Update()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            mousePos = RuntimePanelUtils.ScreenToPanel(root.panel, mousePos);
            _currentMousePosition = mousePos;

            if (_isDragging)
            {
                _draggedItem.style.left =
                    mousePos.x - (_draggingProperties.dragRelativePosition.x + 0.33f) * _cellWidth;
                _draggedItem.style.top = root.resolvedStyle.height - mousePos.y -
                                         (_draggingProperties.dragRelativePosition.y + 0.33f) * _cellHeight;

                if (_draggedItem.style.display.value == DisplayStyle.None)
                {
                    _draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                }
            }
            
            _infoBoxViewer.Update(mousePos);
        }

        public override void Refresh()
        {
            inventoryContainer.Clear();
            Array.Clear(_inventoryCells, 0, _inventoryCells.Length);

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

                    Vector2Int position = new(col, row);
                    if (!_inventory.ValidatePosition(position))
                    {
                        cell.visible = false;
                        previousID = 0;
                        rowElement.Add(cell);

                        continue;
                    }

                    // Get sprite from item, and rotate it if necessary
                    RelativePositionAndID relativePositionAndID = _inventory.GetRelativePositionAt(position);
                    Item item = _inventory.GetAt(position);

                    if (relativePositionAndID != null)
                    {
                        RenderIconSquare(relativePositionAndID.relativePosition, relativePositionAndID.rotation,
                            iconElement, item);
                        MergeBorders(relativePositionAndID, background, position, previousID);
                        
                        if (canMove)
                        {
                            cell.RegisterCallback<MouseDownEvent>(evt =>
                            {
                                if (evt.button == 0)
                                {
                                    ProcessMouseDownLeftCell(position);
                                }
                            });
                        }

                        if (canOpenContextMenu)
                        {
                            cell.RegisterCallback<MouseUpEvent>(evt =>
                            {
                                if (evt.button == 1)
                                {
                                    ProcessMouseUpRightCell(evt, item, relativePositionAndID.itemID);
                                }
                            });
                        }

                        cell.RegisterCallback<MouseEnterEvent>(_ => OpenItemInfo(item));
                        cell.RegisterCallback<MouseLeaveEvent>(_ => CloseItemInfo());
                        previousID = relativePositionAndID.itemID;
                    }
                    else
                    {
                        previousID = 0;
                    }

                    cell.RegisterCallback<MouseUpEvent>(evt => ProcessMouseUpCell(evt, position));

                    if (!_registeredGeometryChange)
                    {
                        cell.RegisterCallback<GeometryChangedEvent>(evt =>
                        {
                            _cellWidth = cell.resolvedStyle.width;
                            _cellHeight = cell.resolvedStyle.height;
                        });
                        _registeredGeometryChange = true;
                    }

                    rowElement.Add(cell);
                }

                inventoryContainer.Add(rowElement);
            }
        }

        private void CloseContext()
        {
            _contextMenuViewer.Close();
        }

        private void ProcessMouseUpRightCell(EventBase evt, Item item, uint itemID)
        {
            if (_isDragging)
            {
                return;
            }

            if (_contextMenuViewer.IsOpen && _contextMenuViewer.ItemInfoID == itemID)
            {
                CloseContext();
                return;
            }
            evt.StopPropagation();

            Vector2 position = _currentMousePosition;
            CloseItemInfo();
            _contextMenuViewer.Open(item, itemID, position);
        }
        
        private void OpenItemInfo(Item item)
        {
            if (_isDragging || _contextMenuViewer.IsOpen)
            {
                return;
            }

            _infoBoxViewer.Open(item);
        }

        private void CloseItemInfo()
        {
            _infoBoxViewer.Close();
        }

        public override void Rotate(int direction)
        {
            if (!_isDragging)
            {
                return;
            }

            _draggingProperties.currentRotation = (_draggingProperties.currentRotation + direction) % 4;
            _draggingProperties.dragRelativePosition = ItemGrid<bool>.RotatePointMultiple(
                _draggingProperties.dragRelativePosition, direction);
            RenderItemDrag();
        }

        private void ProcessMouseDownLeftCell(Vector2Int position)
        {
            if (_contextMenuViewer.IsOpen)
            {
                return;
            }

            Item item = _inventory.GetAt(position);
            if (item == null)
            {
                return;
            }

            _isDragging = true;
            CloseItemInfo();
            RelativePositionAndID relativePositionAndID = _inventory.GetRelativePositionAt(position);
            _draggingProperties = new DraggingProperties(new ItemPosition(position, relativePositionAndID.rotation),
                relativePositionAndID.relativePosition, item, relativePositionAndID.itemID);
            uint itemID = relativePositionAndID.itemID;

            DarkenItem(itemID);
            RenderItemDrag();
            onDragStart?.Invoke(_draggingProperties);
        }

        public override void HandleDragStart(IDraggable draggable)
        {
            _otherDraggable = draggable;
        }

        public override void HandleDragEnd(IDraggable draggable)
        {
            if (_isDragging)
            {
                // item successfully dropped on another inventory
                if(draggable is not DraggingProperties draggingProperties) return;
            
                _inventory.RemoveAt(draggingProperties.dragStartPosition.position);
            
                _isDragging = false;
                _draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                if (refreshAfterMove)
                {
                    Refresh();
                }
            }
            else
            {
                _otherDraggable = null;
            }
        }

        private void ProcessMouseUpRoot(MouseUpEvent evt) 
        {
            if (_isDragging)
            {
                _isDragging = false;
                _draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                DarkenItem(_inventory.GetRelativePositionAt(_draggingProperties.dragStartPosition.position).itemID,
                    false);
                onDragEnd?.Invoke(_draggingProperties);
            }
            else if (_contextMenuViewer.IsOpen)
            {
                CloseContext();
            }
        }

        private void RenderItemDrag()
        {
            _draggedItem.Clear();
            _draggedItem.pickingMode = PickingMode.Ignore;

            bool[,] itemGrid = ItemGrid<bool>.RotateMultiple(_draggingProperties.draggedItem.Grid,
                _draggingProperties.currentRotation);

            for (int y = 0; y < ItemConstants.ItemHeight; y++)
            {
                VisualElement row = new();
                row.AddToClassList("row");
                row.pickingMode = PickingMode.Ignore;

                for (int x = 0; x < ItemConstants.ItemWidth; x++)
                {
                    VisualElement cell = new();
                    cell.AddToClassList("item-square");
                    cell.pickingMode = PickingMode.Ignore;

                    VisualElement iconElement = new();
                    iconElement.name = "ItemIcon";
                    iconElement.AddToClassList("icon");
                    iconElement.pickingMode = PickingMode.Ignore;

                    cell.Add(iconElement);

                    if (itemGrid[y, x])
                    {
                        RenderIconSquare(new Vector2Int(x, y), _draggingProperties.currentRotation, iconElement,
                            _draggingProperties.draggedItem);
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
                    RelativePositionAndID relativePositionAndID =
                        _inventory.GetRelativePositionAt(new Vector2Int(x, y));

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

        private void ProcessOtherDraggable(EventBase evt, Vector2Int position, DraggingProperties draggingProperties)
        {
            bool[,] itemGrid = ItemGrid<bool>.RotateMultiple(draggingProperties.draggedItem.Grid,
                draggingProperties.currentRotation);
            BoundsInt bounds = ItemGrid<bool>.GetBounds(itemGrid, true);

            Vector2Int initialPosition =
                position - (draggingProperties.dragRelativePosition - new Vector2Int(bounds.x, bounds.y));

            if (!_inventory.CheckFit(draggingProperties.draggedItem, initialPosition,
                    draggingProperties.currentRotation))
            {
                return;
            }
            evt.StopPropagation();

            _inventory.AddItem(draggingProperties.Item, initialPosition, draggingProperties.currentRotation);
            
            if (refreshAfterMove)
            {
                Refresh();
            }
            onDragEnd?.Invoke(draggingProperties);
        }
        
        private void ProcessOtherDraggableSimple(EventBase evt, Vector2Int position, IDraggable draggingProperties)
        {
            if (!_inventory.CheckFit(draggingProperties.Item, position, 0))
            {
                return;
            }
            evt.StopPropagation();

            _inventory.AddItem(draggingProperties.Item, position, 0);
            if (refreshAfterMove)
            {
                Refresh();
            }

            onDragEnd?.Invoke(draggingProperties);
        }

        private void ProcessMouseUpCell(EventBase evt, Vector2Int position)
        {
            if (_otherDraggable != null)
            {
                if (_otherDraggable is DraggingProperties draggingProperties)
                {
                    ProcessOtherDraggable(evt, position, draggingProperties);
                }
                else
                {
                    ProcessOtherDraggableSimple(evt, position, _otherDraggable);
                }
                
                _otherDraggable = null;
                return;
            }
            
            if (!_isDragging)
            {
                return;
            }

            bool[,] itemGrid = ItemGrid<bool>.RotateMultiple(_draggingProperties.draggedItem.Grid,
                _draggingProperties.currentRotation);
            BoundsInt bounds = ItemGrid<bool>.GetBounds(itemGrid, true);

            Vector2Int initialPosition =
                position - (_draggingProperties.dragRelativePosition - new Vector2Int(bounds.x, bounds.y));

            if (!_inventory.CheckFit(_draggingProperties.draggedItem, initialPosition,
                    _draggingProperties.currentRotation, _draggingProperties.itemID))
            {
                return;
            }

            evt.StopPropagation();

            _inventory.MoveItem(_draggingProperties.dragStartPosition,
                new ItemPosition(initialPosition, _draggingProperties.currentRotation));
            _isDragging = false;
            _draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            if (refreshAfterMove)
            {
                Refresh();
            }
            onDragEnd?.Invoke(_draggingProperties);
        }


        private void MergeBorders(RelativePositionAndID relativePositionAndID, VisualElement cell, Vector2Int position,
            uint previousID)
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

            RelativePositionAndID relativePositionAndID = _inventory.GetRelativePositionAt(position);
            return relativePositionAndID != null && relativePositionAndID.itemID == itemID;
        }
    }
}