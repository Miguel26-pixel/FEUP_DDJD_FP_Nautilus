using System;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Inventory
{
    public class PlayerInventory : InventoryGrid, IInventoryNotifier
    {
        private readonly List<IInventorySubscriber> _subscribers = new();
        private bool _notify = true;
        
        public bool Locked { get; set; }

        public PlayerInventory(string inventoryName, bool[,] gridShape) : base(gridShape, inventoryName)
        {
            if (gridShape.GetLength(1) > InventoryConstants.PlayerInventoryMaxWidth ||
                gridShape.GetLength(0) > InventoryConstants.PlayerInventoryMaxHeight)
            {
                throw new ArgumentException("Inventory grid is too large.");
            }
        }

        public PlayerInventory(InventoryGrid inventory) : base(inventory)
        {
            
        }

        public void AddSubscriber(IInventorySubscriber subscriber)
        {
            _subscribers.Add(subscriber);
        }

        public void NotifySubscribersOnInventoryChanged()
        {
            if (!_notify)
            {
                return;
            }
            
            foreach (IInventorySubscriber subscriber in _subscribers)
            {
                subscriber.OnInventoryChanged();
            }
        }

        public void RemoveSubscriber(IInventorySubscriber subscriber)
        {
            _subscribers.Remove(subscriber);
        }

        public override Item RemoveItem(int itemHash)
        {
            if (Locked)
            {
                throw new InvalidOperationException("Player inventory is locked");
            }
            
            Item item = base.RemoveItem(itemHash);
            NotifySubscribersOnInventoryChanged();
            return item;
        }

        public override Item RemoveAt(Vector2Int position)
        {
            if (Locked)
            {
                throw new InvalidOperationException("Player inventory is locked");
            }
            
            Item item = base.RemoveAt(position);
            NotifySubscribersOnInventoryChanged();
            return item;
        }

        public override bool AddItem(Item item)
        {
            if (Locked)
            {
                return false;
            }
            
            bool added = base.AddItem(item);

            if (added)
            {
                NotifySubscribersOnInventoryChanged();
            }
            
            return added;
        }


        public override void AddItem(Item item, Vector2Int position, int rotation)
        {
            if (Locked)
            {
                // TODO: drop item, might need to be handled by the caller not sure yet
            }
            
            
            base.AddItem(item, position, rotation);
            NotifySubscribersOnInventoryChanged();
        }

        public override void MoveItem(ItemPosition source, ItemPosition destination)
        {
            if (Locked)
            {
                return;
            }

            _notify = false;
            base.MoveItem(source, destination);
            _notify = true;
            NotifySubscribersOnInventoryChanged();
        }
    }
}