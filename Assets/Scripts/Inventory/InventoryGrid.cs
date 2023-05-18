using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using UnityEngine;

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
    
    
    public class InventoryGrid
    {
        private readonly bool[,] _gridShape;
        private readonly uint[,] _gridItemIDs;
        private readonly Dictionary<uint, Vector2Int> _itemPositions = new();
        private readonly Dictionary<uint, Item> _items = new();
        private readonly int _width;
        private readonly int _height;
        private uint _itemIDCounter = 0;

        public InventoryGrid(bool[,] gridShape)
        {
            _width = gridShape.GetLength(1);
            _height = gridShape.GetLength(0);
            
            if (_width > InventoryConstants.PlayerInventoryMaxWidth || _height > InventoryConstants.PlayerInventoryMaxHeight)
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

        public void AddItem(Item item, Vector2Int position, int rotation)
        {
            // TODO: ROTATION
            ValidatePosition(position);
            
            if (position.x + item.Bounds.size.x > _width || position.y + item.Bounds.size.y > _height)
            {
                throw new ItemDoesNotFitException("Item does not fit.");
            }
            
            for (int y = position.y; y < position.y + item.Bounds.size.y; y++)
            {
                for (int x = position.x; x < position.x + item.Bounds.size.x; x++)
                {
                    if (!item.Grid[y - position.y + item.Bounds.y, x - position.x + item.Bounds.x]) continue;
                    
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
            _itemPositions.Add(itemID, position);
            
            for (int y = position.y; y < position.y + item.Bounds.size.y; y++)
            {
                for (int x = position.x; x < position.x + item.Bounds.size.x; x++)
                {
                    if (!item.Grid[y - position.y + item.Bounds.y, x - position.x + item.Bounds.x]) continue;                    
                    
                    _gridItemIDs[y, x] = itemID;
                }
            }
        }

        public void RemoveItem(int itemHash)
        {
            var pair = _items.FirstOrDefault(pair => pair.Value.IDHash == itemHash);

            if (pair.Equals(default(KeyValuePair<uint, Item>)))
            {
                throw new ItemNotInInventoryException("Item not found.");
            }
            
            uint itemID = pair.Key;
            
            Vector2Int itemPosition = _itemPositions[itemID];
            
            for (int y = itemPosition.y; y < itemPosition.y + ItemConstants.ItemHeight && y < _height; y++)
            {
                for (int x = itemPosition.x; x < itemPosition.x + ItemConstants.ItemWidth && x < _width; x++)
                {
                    if (!_gridShape[y, x]) continue;
                    if (_gridItemIDs[y, x] != itemID) continue;

                    _gridItemIDs[y, x] = 0;
                }
            }
            
            _items.Remove(itemID);
            _itemPositions.Remove(itemID);
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