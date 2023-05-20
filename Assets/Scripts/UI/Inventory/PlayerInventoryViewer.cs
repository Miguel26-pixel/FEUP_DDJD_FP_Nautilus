using System;
using System.Collections.Generic;
using Inventory;
using Items;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace UI.Inventory
{
    public record DraggingProperties()
    {
        public readonly Item draggedItem;
        public Vector2Int dragRelativePosition;
        public readonly ItemPosition dragStartPosition;
        public readonly uint itemID;
        public int currentRotation;

        public DraggingProperties(ItemPosition dragStartPosition, Vector2Int dragRelativePosition, Item draggedItem,
            uint itemID) : this()
        {
            this.dragStartPosition = dragStartPosition;
            this.dragRelativePosition = dragRelativePosition;
            this.draggedItem = draggedItem;
            currentRotation = this.dragStartPosition.rotation;
            this.itemID = itemID;
        }
    }

    public class PlayerInventoryViewer : InventoryViewer<PlayerInventory>
    {
        private readonly VisualElement _draggedItem;
        private readonly VisualElement _itemInfo;
        private readonly Label _itemInfoName;
        private readonly Label _itemInfoDescription;
        private readonly VisualElement _itemInfoStats;
        private readonly VisualElement _itemInfoDescriptors;
        private readonly PlayerInventory _inventory;
        private readonly BoundsInt _inventoryBounds;

        private readonly VisualElement[,] _inventoryCells =
            new VisualElement[InventoryConstants.PlayerInventoryMaxHeight,
                InventoryConstants.PlayerInventoryMaxWidth];

        private float _cellHeight;
        private float _cellWidth;
        private DraggingProperties _draggingProperties;

        private bool _isDragging;
        private bool _registeredGeometryChange;
        
        private bool _isInfoVisible;

        public PlayerInventoryViewer(VisualElement root, VisualElement inventoryContainer, VisualTreeAsset itemDescriptorTemplate, PlayerInventory inventory) :
            base(
                root, inventoryContainer, itemDescriptorTemplate, inventory)
        {
            _inventory = inventory;
            _inventoryBounds = _inventory.GetBounds();
            _draggedItem = root.Q<VisualElement>("ItemDrag");
            _draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            _itemInfo = root.Q<VisualElement>("ItemInfo");
            _itemInfo.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            
            _itemInfoName = _itemInfo.Q<Label>("InfoTitle");
            _itemInfoDescription = _itemInfo.Q<Label>("InfoDescription");
            _itemInfoStats = _itemInfo.Q<VisualElement>("ItemInfoStats");
            _itemInfoDescriptors = _itemInfo.Q<VisualElement>("Descriptors");
        }

        public override void Show()
        {
            _registeredGeometryChange = false;

            Refresh();
        }

        public override void Update()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            mousePos = RuntimePanelUtils.ScreenToPanel(root.panel, mousePos);
            if (_isDragging)
            {
                _draggedItem.style.left = mousePos.x - (_draggingProperties.dragRelativePosition.x + 0.33f) * _cellWidth;
                _draggedItem.style.top = root.resolvedStyle.height - mousePos.y -
                                         (_draggingProperties.dragRelativePosition.y + 0.33f) * _cellHeight;
            } else if (_isInfoVisible)
            {
                _itemInfo.style.left = mousePos.x + 3;
                _itemInfo.style.top = root.resolvedStyle.height - mousePos.y - 3;
            }

        }

        public override void Refresh()
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

                    Vector2Int position = new(col, row);
                    if (!_inventory.ValidatePosition(position))
                    {
                        cell.visible = false;
                        previousID = 0;
                        rowElement.Add(cell);

                        continue;
                    }

                    // Get sprite from item, and rotate it if necessary
                    RelativePositionAndID relativePositionAndID = _inventory.GetItemPositionAt(position);
                    Item item = _inventory.GetAt(position);

                    if (relativePositionAndID != null)
                    {
                        RenderIconSquare(relativePositionAndID.relativePosition, relativePositionAndID.rotation,
                            iconElement, _inventory.GetAt(position));
                        MergeBorders(relativePositionAndID, background, position, previousID);
                        cell.RegisterCallback<MouseDownEvent>(_ => ProcessCellClick(position));
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

        private void CloseItemInfo()
        {
            _isInfoVisible = false;
            _itemInfo.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }

        private void OpenItemInfo(Item item)
        {
            if (_isDragging)
            {
                return;
            }
            
            _isInfoVisible = true;
            
            _itemInfoName.text = item.Name;
            _itemInfoDescription.text = item.Description;

            var descriptors = item.GetDescriptors();

            if (descriptors.Count == 0)
            {
                _itemInfoStats.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }
            else
            {
                _itemInfoStats.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                _itemInfoDescriptors.Clear();
                foreach (var descriptor in descriptors)
                {
                    VisualElement descriptorElement = itemDescriptorTemplate.Instantiate();
                    descriptorElement.Q<Label>("DescriptorKey").text = descriptor.Key;
                    descriptorElement.Q<Label>("DescriptorValue").text = descriptor.Value;
                    descriptorElement.pickingMode = PickingMode.Ignore;
                    
                    _itemInfoDescriptors.Add(descriptorElement);
                }
            }
            
            _itemInfo.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
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

        private void ProcessCellClick(Vector2Int position)
        {
            Item item = _inventory.GetAt(position);
            if (item == null)
            {
                return;
            }

            _isDragging = true;
            CloseItemInfo();
            RelativePositionAndID relativePositionAndID = _inventory.GetItemPositionAt(position);
            _draggingProperties = new DraggingProperties(new ItemPosition(position, relativePositionAndID.rotation),
                relativePositionAndID.relativePosition, item, relativePositionAndID.itemID);
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
            DarkenItem(_inventory.GetItemPositionAt(_draggingProperties.dragStartPosition.position).itemID, false);
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

        private void ProcessMouseUpCell(EventBase evt, Vector2Int position)
        {
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
            Refresh();
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

            RelativePositionAndID relativePositionAndID = _inventory.GetItemPositionAt(position);
            return relativePositionAndID != null && relativePositionAndID.itemID == itemID;
        }
    }
}