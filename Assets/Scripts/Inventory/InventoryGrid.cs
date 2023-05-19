using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using JetBrains.Annotations;
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
    
    public record ItemPositionAndID
    {
        public readonly Vector2Int relativePosition;
        public readonly int rotation;
        public readonly uint itemID;

        public ItemPositionAndID(Vector2Int relativePosition, int rotation, uint itemID)
        {
            this.relativePosition = relativePosition;
            this.rotation = rotation;
            this.itemID = itemID;
        }
    }


    public class InventoryGrid
    {
        [ItemCanBeNull] private readonly ItemPositionAndID[,] _gridItemIDs;
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

            _gridShape = gridShape;
            _gridItemIDs = new ItemPositionAndID[_height, _width];
        }
        
        public ItemPositionAndID GetItemPositionAt(Vector2Int position)
        {
            return _gridItemIDs[position.y, position.x];
        }

        public List<Item> GetItems()
        {
            return _items.Values.ToList();
        }

        private Vector2Int FindEmptyPosition(Item item, int rotation)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (!_gridShape[y, x])
                    {
                        continue;
                    }

                    Vector2Int position = new(x, y);
                    if (CheckFit(item, position, rotation))
                    {
                        return position;
                    }
                }
            }

            throw new ItemDoesNotFitException("Item does not fit in inventory.");
        }


        public bool ValidatePosition(Vector2Int position)
        {
            if (position.x < 0 || position.x >= _width || position.y < 0 || position.y >= _height)
            {
                return false;
            }

            return _gridShape[position.y, position.x];
        }

        private Tuple<bool[,], BoundsInt> CheckFitAndGetBounds(Item item, Vector2Int position, int rotation)
        {
            bool[,] itemGrid = ItemGrid<bool>.RotateMultiple(item.Grid, rotation);
            BoundsInt bounds = ItemGrid<bool>.GetBounds(itemGrid, true);

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

                    if (_gridItemIDs[y, x] != null)
                    {
                        throw new PositionAlreadyOccupiedException("Item position is already occupied");
                    }
                }
            }

            return new Tuple<bool[,], BoundsInt>(itemGrid, bounds);
        }

        public void AddItem(Item item)
        {
            Vector2Int position = FindEmptyPosition(item, 0);

            AddItem(item, position, 0);
        }

        public bool CheckFit(Item item, Vector2Int position, int rotation)
        {
            try
            {
                CheckFitAndGetBounds(item, position, rotation);
                return true;
            }
            catch (ItemDoesNotFitException)
            {
                return false;
            }
            catch (PositionAlreadyOccupiedException)
            {
                return false;
            }
        }

        // rotation is in increments of 90 degrees, positive is counter-clockwise, negative is clockwise
        public void AddItem(Item item, Vector2Int position, int rotation)
        {
            if (!ValidatePosition(position))
            {
                throw new InvalidItemPositionException("Item position is out of bounds.");
            }

            (bool[,] itemGrid, BoundsInt bounds) = CheckFitAndGetBounds(item, position, rotation);

            uint itemID = ++_itemIDCounter;

            _items.Add(itemID, item);
            _itemPositions.Add(itemID, new ItemPosition(position, rotation));

            for (int y = position.y; y < position.y + bounds.size.y; y++)
            {
                for (int x = position.x; x < position.x + bounds.size.x; x++)
                {
                    int yPosition = y - position.y + bounds.y;
                    int xPosition = x - position.x + bounds.x;
                    
                    if (!itemGrid[yPosition, xPosition])
                    {
                        continue;
                    }

                    _gridItemIDs[y, x] = new ItemPositionAndID(new Vector2Int(xPosition, yPosition), rotation, itemID);
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
            Item item = pair.Value;

            RemoveAtInternal(_itemPositions[itemID].position, itemID);

            return item;
        }

        public Item GetAt(Vector2Int position)
        {
            if (!ValidatePosition(position))
            {
                throw new InvalidItemPositionException("Item position is out of bounds.");
            }

            ItemPositionAndID positionAndID = _gridItemIDs[position.y, position.x];

            return positionAndID == null ? null : _items[positionAndID.itemID];
        }

        // ONLY USE WITH ROOT POSITION
        private void RemoveAtInternal(Vector2Int itemPosition, uint itemID)
        {
            for (int y = itemPosition.y; y < itemPosition.y + ItemConstants.ItemHeight && y < _height; y++)
            {
                for (int x = itemPosition.x; x < itemPosition.x + ItemConstants.ItemWidth && x < _width; x++)
                {
                    if (!_gridShape[y, x])
                    {
                        continue;
                    }

                    if (_gridItemIDs[y, x] == null || _gridItemIDs[y, x].itemID != itemID)
                    {
                        continue;
                    }

                    _gridItemIDs[y, x] = null;
                }
            }

            _items.Remove(itemID);
            _itemPositions.Remove(itemID);
        }

        public Item RemoveAt(Vector2Int position)
        {
            if (!ValidatePosition(position))
            {
                throw new InvalidItemPositionException("Item position is out of bounds.");
            }

            ItemPositionAndID itemPositionAndID = _gridItemIDs[position.y, position.x];

            if (itemPositionAndID == null)
            {
                throw new ItemNotInInventoryPositionException("No item at position.");
            }

            Item item = _items[itemPositionAndID.itemID];

            ItemPosition itemPositionObject = _itemPositions[itemPositionAndID.itemID];
            Vector2Int itemPosition = itemPositionObject.position;

            RemoveAtInternal(itemPosition, itemPositionAndID.itemID);

            return item;
        }

        public void MoveItem(ItemPosition source, ItemPosition destination)
        {
            Item sourceItem = GetAt(source.position);
            if (sourceItem == null)
            {
                throw new ItemNotInInventoryPositionException("No item at source position.");
            }

            // Check fit by adding the item to the destination position, then removing it
            // Throws an exception if the item does not fit
            AddItem(sourceItem, destination.position, destination.rotation);
            RemoveAt(source.position);
        }
    }
}