using System;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Inventory
{
    public class PlayerInventory : InventoryGrid, IInventoryNotifier
    {
        private readonly List<IInventorySubscriber> _subscribers = new();

        public PlayerInventory(string inventoryName, bool[,] gridShape) : base(gridShape, inventoryName)
        {
            if (gridShape.GetLength(1) > InventoryConstants.PlayerInventoryMaxWidth ||
                gridShape.GetLength(0) > InventoryConstants.PlayerInventoryMaxHeight)
            {
                throw new ArgumentException("Inventory grid is too large.");
            }
        }

        public void AddSubscriber(IInventorySubscriber subscriber)
        {
            _subscribers.Add(subscriber);
        }
        
        public void RemoveSubscriber(IInventorySubscriber subscriber)
        {
            _subscribers.Remove(subscriber);
        }

        public void NotifySubscribersOnInventoryChanged()
        {
            foreach (IInventorySubscriber subscriber in _subscribers)
            {
                subscriber.OnInventoryChanged();
            }
        }

        public override Item RemoveItem(int itemID)
        {
            Item item = base.RemoveItem(itemID);
            NotifySubscribersOnInventoryChanged();
            return item;
        }

        public override void AddItem(Item item)
        {
            try
            {
                base.AddItem(item);
            }
            catch (ItemDoesNotFitException)
            {
                // TODO: drop item, might need to be handled by the caller not sure yet
            }

            NotifySubscribersOnInventoryChanged();
        }


        public override void AddItem(Item item, Vector2Int position, int rotation)
        {
            base.AddItem(item, position, rotation);
            NotifySubscribersOnInventoryChanged();
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