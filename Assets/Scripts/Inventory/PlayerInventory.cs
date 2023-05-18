using System;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Inventory
{
    public class PlayerInventory : IInventory, IInventoryNotifier
    {
        private readonly string _inventoryName;
        private readonly List<Item> _items = new();
        private readonly List<IInventorySubscriber> _subscribers = new();

        public PlayerInventory(string inventoryName)
        {
            _inventoryName = inventoryName;
        }

        public List<Item> GetItems()
        {
            return _items;
        }

        public Item RemoveItem(int itemID)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].IDHash == itemID)
                {
                    Item item = _items[i];
                    _items.RemoveAt(i);
                    NotifySubscribersOnInventoryChanged();

                    return item;
                }
            }

            return null;
        }

        public void AddItem(Item item, Vector2Int position, int rotation)
        {
            _items.Add(item);
        }

        public void TransferItems(IInventory destination, TransferDirection direction)
        {
            // TODO: Implement this
            // Open transfer window
            // Transfer items
            // Close transfer window
            // Notify subscribers
            if (direction == TransferDirection.DestinationToSource)
            {
                _items.AddRange(destination.GetItems());
            }
            else
            {
                throw new NotImplementedException();
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
    }
}