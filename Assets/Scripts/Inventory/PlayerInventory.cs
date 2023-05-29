using System;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Inventory
{
    public class IntermediateResource
    {
        public IntermediateResource(ItemData item)
        {
            NeededCollectionCount = item.GetComponent<ResourceComponentData>().NeededCollectionCount;
            Count = 0;
        }

        public int NeededCollectionCount { get; }
        public int Count { get; private set; }

        public bool IsFull()
        {
            return Count == NeededCollectionCount - 1;
        }

        public void Increment()
        {
            if (!IsFull())
            {
                Count++;
            }
        }
    }

    public class SoilResource
    {
        public SoilResource(int maxCount)
        {
            MaxCount = maxCount;
            Count = 0;
        }

        public int MaxCount { get; }
        public float Count { get; private set; }
        
        public bool IsFull()
        {
            return Count >= MaxCount;
        }
        
        public void Increment(float amount)
        {
            if (!IsFull())
            {
                Count += amount;
            }
            
            if (Count < 0)
            {
                Count = 0;
            }
        }

        public void SetValue(float value)
        {
            if (value < 0)
            {
                value = 0;
            }
            else if (value > MaxCount)
            {
                value = MaxCount;
            }
            
            Count = value;
        }

        public void Reset()
        {
            Count = 0;
        }
    }

    public class PlayerInventory : InventoryGrid, IInventoryNotifier
    {
        private readonly Dictionary<int, IntermediateResource> _intermediateResources = new();
        private SoilResource _soil = null;

        private readonly List<IInventorySubscriber> _subscribers = new();

        private bool _notify = true;

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

        public bool Locked { get; set; }

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

        public IntermediateResource GetIntermediateResource(int itemID)
        {
            return _intermediateResources.TryGetValue(itemID, out IntermediateResource intermediateResource)
                ? intermediateResource
                : null;
        }
        
        public SoilResource GetSoilResource()
        {
            return _soil;
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
        
        public bool RemoveSoil(ItemData soilData, float amount)
        {
            int maxSoil = soilData.GetComponent<ResourceComponentData>().NeededCollectionCount;
            
            float curAmount = _soil?.Count ?? 0;
            float neededAmount = Mathf.Max(0, amount - curAmount);

            int neededSoil = Mathf.CeilToInt(neededAmount / maxSoil);
            float remainingAmount = (curAmount + neededSoil * maxSoil) - amount;
            
            if (neededSoil > ItemCount(soilData.IDHash))
            {
                return false;
            }
            
            for (int i = 0; i < neededSoil; i++)
            {
                RemoveItem(soilData.IDHash);
            }

            _soil ??= new SoilResource(maxSoil);
            _soil.SetValue(remainingAmount);
            return true;
        }

        public bool AddSoil(ItemData soilData, float amount)
        {
            _soil ??= new SoilResource(soilData.GetComponent<ResourceComponentData>().NeededCollectionCount);
            _soil.Increment(amount);

            if (!_soil.IsFull())
            {
                return true;
            }

            if (!AddInternal(soilData.CreateInstance()))
            {
                return false;
            }

            _soil = null;
            return true;
        }

        public bool AddResource(ItemData item)
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

            if (!AddInternal(item.CreateInstance()))
            {
                return false;
            }

            _intermediateResources.Remove(item.IDHash);
            return true;
        }

        public override bool AddItem(Item item)
        {
            return AddInternal(item);
        }


        public override void AddItem(Item item, Vector2Int position, int rotation)
        {
            if (Locked)
            {
                return;
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