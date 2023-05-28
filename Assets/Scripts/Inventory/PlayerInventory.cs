using System;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Inventory
{
    public class IntermediateResource
    {
        private readonly int _neededCollectionCount;
        private int _count;
        
        public IntermediateResource(Item item)
        {
            _neededCollectionCount = item.GetComponent<ResourceComponent>().NeededCollectionCount - 1;
            _count = 0;
        }
        
        public bool IsFull()
        {
            return _count == _neededCollectionCount;
        }
        
        public void Increment()
        {
            if (!IsFull())
            {
                _count++;
            }
        }
    }
    
    public class PlayerInventory : InventoryGrid, IInventoryNotifier
    {
        private readonly List<IInventorySubscriber> _subscribers = new();
        private readonly Dictionary<int, IntermediateResource> _intermediateResources = new();

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

        private bool AddInternal(Item item)
        {
            if (Locked)
            {
                return false;
            }

            if (!base.AddItem(item))
            {
                return false;
            }

            NotifySubscribersOnInventoryChanged();
            return true;
        }
        
        private bool AddResource(Item item)
        {
            if (!_intermediateResources.TryGetValue(item.IDHash, out IntermediateResource intermediateResource))
            {
                intermediateResource = new IntermediateResource(item);
                _intermediateResources.Add(item.IDHash, intermediateResource);
            }
            
            if (!intermediateResource.IsFull())
            {
                intermediateResource.Increment();
                return true;
            }

            if (!AddInternal(item))
            {
                return false;
            }

            _intermediateResources.Remove(item.IDHash);
            return true;
        }

        public override bool AddItem(Item item)
        {
            if (Locked)
            {
                return false;
            }

            return item.HasComponent<ResourceComponent>() ? AddResource(item) : AddInternal(item);
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