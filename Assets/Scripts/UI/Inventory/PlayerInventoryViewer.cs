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
        private bool _isContextOpen;
        private readonly VisualElement _itemContext;
        private readonly Label _contextTitle;
        private readonly VisualElement _closeContext;
        private readonly VisualElement _contextActions;
        private readonly Label _noActionsLabel;
        private readonly VisualTreeAsset _textButtonTemplate;
        
        private readonly PlayerInventory _inventory;
        private readonly BoundsInt _inventoryBounds;
        
        private readonly VisualElement[,] _inventoryCells =
            new VisualElement[InventoryConstants.PlayerInventoryMaxHeight,
                InventoryConstants.PlayerInventoryMaxWidth];

        private float _cellHeight;
        private float _cellWidth;
        
        private readonly VisualElement _draggedItem;
        private DraggingProperties _draggingProperties;
        private bool _isDragging;
        
        private bool _registeredGeometryChange;
        
        private bool _isInfoVisible;
        private readonly VisualElement _itemInfo;
        private readonly Label _itemInfoName;
        private readonly Label _itemInfoDescription;
        private readonly VisualElement _itemInfoStats;
        private readonly VisualElement _itemInfoDescriptors;

        private Vector2 _currentMousePosition;

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
            
            _textButtonTemplate = Resources.Load<VisualTreeAsset>("UI/TextButton");
            _itemContext = root.Q<VisualElement>("ItemContext");
            
            _contextTitle = _itemContext.Q<Label>("ContextTitle");
            _closeContext = _itemContext.Q<VisualElement>("CloseContext");
            _contextActions = _itemContext.Q<VisualElement>("ContextActions");
            _noActionsLabel = _itemContext.Q<Label>("NoActions");
            _itemContext.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            
            _closeContext.RegisterCallback((MouseUpEvent evt) =>
            {
                CloseContext();
            });
        }
        

        public override void Show()
        {
            _registeredGeometryChange = false;

            Refresh();
        }

        private void SetTopLeft(Vector2 mousePos, VisualElement element)
        {
            if (mousePos.y - element.resolvedStyle.height < 0)
            {
                element.style.bottom = mousePos.y - 3;
                element.style.top = new StyleLength(StyleKeyword.Auto);
            }
            else
            {        
                element.style.top = root.resolvedStyle.height - mousePos.y + 3;
                element.style.bottom = new StyleLength(StyleKeyword.Auto);
            }

            if (mousePos.x + element.resolvedStyle.width > root.resolvedStyle.width)
            {
                element.style.right = root.resolvedStyle.width - mousePos.x - 3;
                element.style.left = new StyleLength(StyleKeyword.Auto);
            }
            else
            {
                element.style.left = mousePos.x + 3;
                element.style.right = new StyleLength(StyleKeyword.Auto);
            }
        }

        public override void Update()
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            mousePos = RuntimePanelUtils.ScreenToPanel(root.panel, mousePos);
            _currentMousePosition = mousePos;
            
            if (_isDragging)
            {
                _draggedItem.style.left = mousePos.x - (_draggingProperties.dragRelativePosition.x + 0.33f) * _cellWidth;
                _draggedItem.style.top = root.resolvedStyle.height - mousePos.y -
                                         (_draggingProperties.dragRelativePosition.y + 0.33f) * _cellHeight;
                
                if (_draggedItem.style.display.value == DisplayStyle.None)
                {
                    _draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                }
            } else if (_isInfoVisible)
            {
                if (_itemInfo.resolvedStyle.height == 0 || _itemInfo.resolvedStyle.width == 0)
                {
                    return;
                }
                
                SetTopLeft(mousePos, _itemInfo);
                
                if (_itemInfo.style.visibility.value == Visibility.Hidden)
                {
                    _itemInfo.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
                }
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
                            iconElement, item);
                        MergeBorders(relativePositionAndID, background, position, previousID);
                        cell.RegisterCallback<MouseDownEvent>(evt =>
                        {
                            if(evt.button == 0)
                            {
                                    ProcessCellClick(position);
                            }
                        });
                        cell.RegisterCallback<MouseUpEvent>(evt =>
                        {
                            if (evt.button == 1)
                            {
                                ProcessContextMenu(evt, item);
                            }
                        });
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
            _isContextOpen = false;
            _itemContext.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }

        private void ProcessContextMenu(MouseUpEvent evt, Item item)
        {
            if (_isDragging)
            {
                return;
            }

            Vector2 position = _currentMousePosition;

            CloseItemInfo();
            _isContextOpen = true;
            
            _contextTitle.text = item.Name;
            
            List<ContextMenuAction> actions = item.GetContextMenuActions();
            
            _contextActions.Clear();

            if (actions.Count == 0)
            {
                _noActionsLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            }
            else
            {
                _noActionsLabel.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                
                foreach (ContextMenuAction action in actions)
                {
                    VisualElement textButton = _textButtonTemplate.Instantiate();
                    Label label = textButton.Q<Label>("Label");
                    
                    label.text = action.Name;
                    
                    textButton.RegisterCallback<MouseUpEvent>(evt =>
                    {
                        if (evt.button == 0)
                        {
                            try
                            {
                                action.Action();
                            }
                            catch (NotImplementedException ex)
                            {
                                Debug.Log("Action not implemented");
                            }

                            CloseContext();
                        }
                    });
                    
                    _contextActions.Add(textButton);
                }
            }
            
            _itemContext.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            _itemContext.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
            
            _itemContext.RegisterCallback<GeometryChangedEvent>(evt =>
            {
                SetTopLeft(position, _itemContext);
                
                _itemContext.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
            });
        }

        private void CloseItemInfo()
        {
            _isInfoVisible = false;
            _itemInfo.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }

        private void OpenItemInfo(Item item)
        {
            if (_isDragging || _isContextOpen)
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
            _itemInfo.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
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