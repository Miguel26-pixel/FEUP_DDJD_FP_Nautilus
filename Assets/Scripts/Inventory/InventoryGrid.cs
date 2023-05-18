using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using UnityEngine;
using Utils;

namespace Inventory
{
    public class InvalidItemPositionException : Exception
    {
        public InvalidItemPositionException(string message) : base(message)
        {
        }
    }

    public class ItemDoesNotFitException : Exception
    {
        public ItemDoesNotFitException(string message) : base(message)
        {
        }
    }

    public class PositionAlreadyOccupiedException : Exception
    {
        public PositionAlreadyOccupiedException(string message) : base(message)
        {
        }
    }

    public class ItemNotInInventoryException : Exception
    {
        public ItemNotInInventoryException(string message) : base(message)
        {
        }
    }

    public class ItemNotInInventoryPositionException : Exception
    {
        public ItemNotInInventoryPositionException(string message) : base(message)
        {
        }
    }


    public record ItemPosition
    {
        public readonly Vector2Int position;
        public readonly int rotation;

        public ItemPosition(Vector2Int position, int rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }


    public class InventoryGrid
    {
        private readonly uint[,] _gridItemIDs;
        private readonly bool[,] _gridShape;
        private readonly int _height;
        private readonly Dictionary<uint, ItemPosition> _itemPositions = new();
        private readonly Dictionary<uint, Item> _items = new();
        private readonly int _width;
        private uint _itemIDCounter;

        public InventoryGrid(bool[,] gridShape)
        {
            _width = gridShape.GetLength(1);
            _height = gridShape.GetLength(0);

            if (_width > InventoryConstants.PlayerInventoryMaxWidth ||
                _height > InventoryConstants.PlayerInventoryMaxHeight)
            {
                throw new ArgumentException("Inventory grid is too large.");
            }

            _gridShape = gridShape;
            _gridItemIDs = new uint[_height, _width];
        }

        private void ValidatePosition(Vector2Int position)
        {
            if (position.x < 0 || position.x >= _width || position.y < 0 || position.y >= _height)
            {
                throw new InvalidItemPositionException("Item position is out of bounds.");
            }

            if (!_gridShape[position.y, position.x])
            {
                throw new InvalidItemPositionException("Item position is not valid.");
            }
        }

        // rotation is in increments of 90 degrees, positive is counter-clockwise, negative is clockwise
        public void AddItem(Item item, Vector2Int position, int rotation)
        {
            ValidatePosition(position);
            
            // Rotate the item
            bool[,] itemGrid = item.Grid;

            // rotate 3 is the same as rotate -1, rotate 4 is the same as rotate 0, etc.
            rotation = MathUtils.Modulo(rotation + 2, 4) - 2;
            
            int rotationsLeft = rotation;
            switch (rotationsLeft)
            {
                case > 0:
                {
                    while (rotationsLeft > 0)
                    {
                        itemGrid = ItemGrid.RotateCounterClockwise(itemGrid);
                        rotationsLeft--;
                    }

                    break;
                }
                case < 0:
                {
                    while (rotationsLeft < 0)
                    {
                        itemGrid = ItemGrid.RotateClockwise(itemGrid);
                        rotationsLeft++;
                    }

                    break;
                }
            }
            BoundsInt bounds = ItemGrid.GetBounds(itemGrid);

            if (position.x + bounds.size.x > _width || position.y + bounds.size.y > _height)
            {
                throw new ItemDoesNotFitException("Item does not fit.");
            }

            for (int y = position.y; y < position.y + bounds.size.y; y++)
            {
                for (int x = position.x; x < position.x + bounds.size.x; x++)
                {
                    if (!itemGrid[y - position.y + bounds.y, x - position.x + bounds.x])
                    {
                        continue;
                    }

                    if (!_gridShape[y, x])
                    {
                        throw new ItemDoesNotFitException("Item does not fit.");
                    }

                    if (_gridItemIDs[y, x] != 0)
                    {
                        throw new PositionAlreadyOccupiedException("Item position is already occupied");
                    }
                }
            }

            uint itemID = ++_itemIDCounter;

            _items.Add(itemID, item);
            _itemPositions.Add(itemID, new ItemPosition(position, rotation));

            for (int y = position.y; y < position.y + bounds.size.y; y++)
            {
                for (int x = position.x; x < position.x + bounds.size.x; x++)
                {
                    if (!itemGrid[y - position.y + bounds.y, x - position.x + bounds.x])
                    {
                        continue;
                    }

                    _gridItemIDs[y, x] = itemID;
                }
            }
        }

        public Item RemoveItem(int itemHash)
        {
            KeyValuePair<uint, Item> pair = _items.FirstOrDefault(pair => pair.Value.IDHash == itemHash);

            if (pair.Equals(default(KeyValuePair<uint, Item>)))
            {
                throw new ItemNotInInventoryException("Item not found.");
            }

            uint itemID = pair.Key;

            ItemPosition itemPositionObject = _itemPositions[itemID];
            Vector2Int itemPosition = itemPositionObject.position;

            for (int y = itemPosition.y; y < itemPosition.y + ItemConstants.ItemHeight && y < _height; y++)
            {
                for (int x = itemPosition.x; x < itemPosition.x + ItemConstants.ItemWidth && x < _width; x++)
                {
                    if (!_gridShape[y, x])
                    {
                        continue;
                    }

                    if (_gridItemIDs[y, x] != itemID)
                    {
                        continue;
                    }

                    _gridItemIDs[y, x] = 0;
                }
            }

            Item item = pair.Value;
            _items.Remove(itemID);
            _itemPositions.Remove(itemID);

            return item;
        }

        public Item GetAt(Vector2Int position)
        {
            ValidatePosition(position);
            uint itemID = _gridItemIDs[position.y, position.x];

            return itemID == 0 ? null : _items[itemID];
        }

        public Item RemoveAt(Vector2Int position)
        {
            ValidatePosition(position);
            uint itemID = _gridItemIDs[position.y, position.x];

            if (itemID == 0)
            {
                throw new ItemNotInInventoryPositionException("No item at position.");
            }

            Item item = _items[itemID];

            RemoveItem(item.IDHash);

            return item;
        }
    }
}