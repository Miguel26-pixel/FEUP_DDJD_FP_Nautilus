using System;
using Inventory;
using Items;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using PlayerControls;

namespace UI.Inventory.Components
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
        private readonly ContextMenuViewer _contextMenuViewer;

        protected readonly VisualElement draggedItem;

        private readonly InfoBoxViewer _infoBoxViewer;
        private readonly InventoryGrid _inventory;
        private readonly BoundsInt _inventoryBounds;

        private readonly VisualElement[,] _inventoryCells =
            new VisualElement[InventoryConstants.PlayerInventoryMaxHeight,
                InventoryConstants.PlayerInventoryMaxWidth];

        private float _cellHeight;
        private float _cellWidth;
        private Vector2 _currentMousePosition;
        protected DraggingProperties draggingProperties;
        protected bool isDragging;

        private IDraggable _otherDraggable;

        private bool _registeredGeometryChange;

        public GridInventoryViewer(VisualElement root, VisualElement inventoryContainer,
            InventoryGrid inventory, Player player, Action<IDraggable> onDragStart = null, Action<IDraggable> onDragEnd = null,
            bool canMove = true, bool canOpenContext = true, bool refreshAfterMove = true
        ) :
            base(
                root, inventoryContainer, inventory, player, onDragStart, onDragEnd, canMove, canOpenContext, refreshAfterMove)
        {
            _inventory = inventory;
            _inventoryBounds = _inventory.GetBounds();

            draggedItem = root.Q<VisualElement>("ItemDrag");
            if (draggedItem == null)
            {
                Resources.Load<VisualTreeAsset>("UI/ItemDrag").CloneTree(root);
                draggedItem = root.Q<VisualElement>("ItemDrag");
            }

            draggedItem.style.display = DisplayStyle.None;

            _infoBoxViewer = new InfoBoxViewer(root);
            _contextMenuViewer = new ContextMenuViewer(root);
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

            if (isDragging)
            {
                draggedItem.style.left =
                    mousePos.x - (draggingProperties.dragRelativePosition.x + 0.33f) * _cellWidth;
                draggedItem.style.top = root.resolvedStyle.height - mousePos.y -
                                         (draggingProperties.dragRelativePosition.y + 0.33f) * _cellHeight;

                if (draggedItem.style.display.value == DisplayStyle.None)
                {
                    draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
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
                rowElement.pickingMode = PickingMode.Ignore;
                uint previousID = 0;

                for (int col = _inventoryBounds.x; col < _inventoryBounds.x + _inventoryBounds.size.x; col++)
                {
                    VisualElement cell = new();
                    cell.name = $"Cell{col}";
                    cell.AddToClassList("item-square");

                    VisualElement iconElement = new();
                    iconElement.name = "ItemIcon";
                    iconElement.AddToClassList("icon");
                    iconElement.pickingMode = PickingMode.Ignore;

                    VisualElement overlayElement = new();
                    overlayElement.name = "Overlay";
                    overlayElement.AddToClassList("overlay");
                    overlayElement.pickingMode = PickingMode.Ignore;

                    VisualElement background = new();
                    background.name = "Background";
                    background.AddToClassList("cell-background");
                    background.pickingMode = PickingMode.Ignore;

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
                        cell.RegisterCallback<GeometryChangedEvent>(_ =>
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

        public override void Rotate(int direction)
        {
            if (!isDragging)
            {
                return;
            }

            draggingProperties.currentRotation = (draggingProperties.currentRotation + direction) % 4;
            draggingProperties.dragRelativePosition = ItemGrid<bool>.RotatePointMultiple(
                draggingProperties.dragRelativePosition, direction);
            RenderItemDrag();
        }

        public override void HandleDragStart(IDraggable draggable)
        {
            _otherDraggable = draggable;
        }

        public override void HandleDragEnd(IDraggable draggable)
        {
            if (isDragging)
            {
                // item successfully dropped on another inventory
                if (draggable is not DraggingProperties draggingProperties)
                {
                    return;
                }

                _inventory.RemoveAt(draggingProperties.dragStartPosition.position);

                isDragging = false;
                draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
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

        private void ProcessMouseUpRightCell(EventBase evt, Item item, uint itemID)
        {

            Player player = GameObject.FindWithTag("Player").GetComponent<Player>();

            if (player == null)
            {
                return;
            }

            if (isDragging)
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
            _contextMenuViewer.Open(item, itemID, position, player);
        }

        private void CloseContext()
        {
            _contextMenuViewer.Close();
        }

        private void OpenItemInfo(Item item)
        {
            if (isDragging || _contextMenuViewer.IsOpen)
            {
                return;
            }

            _infoBoxViewer.Open(item);
        }

        private void CloseItemInfo()
        {
            _infoBoxViewer.Close();
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

            isDragging = true;
            CloseItemInfo();
            RelativePositionAndID relativePositionAndID = _inventory.GetRelativePositionAt(position);
            draggingProperties = new DraggingProperties(new ItemPosition(position, relativePositionAndID.rotation),
                relativePositionAndID.relativePosition, item, relativePositionAndID.itemID);
            uint itemID = relativePositionAndID.itemID;

            DarkenItem(itemID);
            RenderItemDrag();
            onDragStart?.Invoke(draggingProperties);
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

            if (!isDragging)
            {
                return;
            }

            bool[,] itemGrid = ItemGrid<bool>.RotateMultiple(draggingProperties.draggedItem.Grid,
                draggingProperties.currentRotation);
            BoundsInt bounds = ItemGrid<bool>.GetBounds(itemGrid, true);

            Vector2Int initialPosition =
                position - (draggingProperties.dragRelativePosition - new Vector2Int(bounds.x, bounds.y));

            if (!_inventory.CheckFit(draggingProperties.draggedItem, initialPosition,
                    draggingProperties.currentRotation, draggingProperties.itemID))
            {
                return;
            }

            evt.StopPropagation();

            _inventory.MoveItem(draggingProperties.dragStartPosition,
                new ItemPosition(initialPosition, draggingProperties.currentRotation));
            isDragging = false;
            draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            onDragEnd?.Invoke(draggingProperties);
            if (refreshAfterMove)
            {
                Refresh();
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

        private void ProcessMouseUpRoot(MouseUpEvent evt)
        {
            if (isDragging)
            {
                isDragging = false;
                draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                DarkenItem(_inventory.GetRelativePositionAt(draggingProperties.dragStartPosition.position).itemID,
                    false);
                onDragEnd?.Invoke(draggingProperties);
            }
            else if (_contextMenuViewer.IsOpen)
            {
                CloseContext();
            }
        }

        private void RenderItemDrag()
        {
            draggedItem.Clear();
            draggedItem.pickingMode = PickingMode.Ignore;

            bool[,] itemGrid = ItemGrid<bool>.RotateMultiple(draggingProperties.draggedItem.Grid,
                draggingProperties.currentRotation);

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
                        RenderIconSquare(new Vector2Int(x, y), draggingProperties.currentRotation, iconElement,
                            draggingProperties.draggedItem);
                    }
                    else
                    {
                        cell.visible = false;
                    }

                    row.Add(cell);
                }

                draggedItem.Add(row);
            }

            draggedItem.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
        }

        protected void DarkenItem(uint itemID, bool darken = true)
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