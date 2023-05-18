using System;
using System.Collections.Generic;
using Items;

namespace Inventory
{
    public class InventoryMock : IInventory, IInventoryNotifier
    {
        private readonly string _inventoryName;
        private readonly List<IInventorySubscriber> _subscribers = new();
        public readonly List<Item> items = new();

        public InventoryMock(string inventoryName)
        {
            _inventoryName = inventoryName;
        }

        public List<Item> GetItems()
        {
            return items;
        }

        public Item RemoveItem(int itemID)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].IDHash == itemID)
                {
                    Item item = items[i];
                    items.RemoveAt(i);
                    NotifySubscribersOnInventoryChanged();

                    return item;
                }
            }

            return null;
        }

        public void AddItem(Item item, int x, int y, int rotation)
        {
            items.Add(item);
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
                items.AddRange(destination.GetItems());
            }
            else
            {
                throw new NotImplementedException();
            }
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