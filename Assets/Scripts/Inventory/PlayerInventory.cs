using System;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Inventory
{
    public class PlayerInventory : IInventory, IInventoryNotifier
    {
        private readonly InventoryGrid _inventoryGrid;
        private readonly string _inventoryName;
        private readonly List<IInventorySubscriber> _subscribers = new();

        public PlayerInventory(string inventoryName, bool[,] gridShape)
        {
            _inventoryName = inventoryName;

            if (gridShape.GetLength(1) > InventoryConstants.PlayerInventoryMaxWidth ||
                gridShape.GetLength(0) > InventoryConstants.PlayerInventoryMaxHeight)
            {
                throw new ArgumentException("Inventory grid is too large.");
            }

            _inventoryGrid = new InventoryGrid(gridShape);
        }
        
        public ItemPositionAndID GetItemPositionAt(Vector2Int position)
        {
            return _inventoryGrid.GetItemPositionAt(position);
        }
        
        public Item GetAt(Vector2Int position)
        {
            return _inventoryGrid.GetAt(position);
        }

        public List<Item> GetItems()
        {
            return _inventoryGrid.GetItems();
        }

        public Item RemoveItem(int itemID)
        {
            Item item = _inventoryGrid.RemoveItem(itemID);
            NotifySubscribersOnInventoryChanged();
            return item;
        }

        public void AddItem(Item item)
        {
            try
            {
                _inventoryGrid.AddItem(item);
            }
            catch (ItemDoesNotFitException)
            {
                // TODO: drop item, might need to be handled by the caller not sure yet
            }

            NotifySubscribersOnInventoryChanged();
        }

        public string GetInventoryName()
        {
            return _inventoryName;
        }

        public void AddSubscriber(IInventorySubscriber subscriber)
        {
            _subscribers.Add(subscriber);
        }

        public void NotifySubscribersOnInventoryChanged()
        {
            foreach (IInventorySubscriber subscriber in _subscribers)
            {
                subscriber.OnInventoryChanged();
            }
        }

        public bool ValidatePosition(Vector2Int position)
        {
            return _inventoryGrid.ValidatePosition(position);
        }

        public void AddItem(Item item, Vector2Int position, int rotation)
        {
            _inventoryGrid.AddItem(item, position, rotation);
            NotifySubscribersOnInventoryChanged();
        }

        public BoundsInt GetBounds()
        {
            return _inventoryGrid.GetBounds();
        }

        public void TransferItems(IInventory destination, TransferDirection direction)
        {
            // TODO: Implement this
            // Open transfer window
            // Transfer items
            // Close transfer window
            // Notify subscribers
            // if (direction == TransferDirection.DestinationToSource)
            // {
            //     _items.AddRange(destination.GetItems());
            // }
            // else
            // {
            //     throw new NotImplementedException();
            // }

            NotifySubscribersOnInventoryChanged();
        }
    }
}