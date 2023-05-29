using System;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Inventory
{
    public class IntermediateResource
    {
        public IntermediateResource(Item item)
        {
            NeededCollectionCount = item.GetComponent<ResourceComponent>().NeededCollectionCount;
            Count = 0;
            Item = item;
        }

        public int NeededCollectionCount { get; }
        public int Count { get; private set; }
        public Item Item { get; private set; }

        public bool IsFull()
        {
            return Count >= NeededCollectionCount;
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
        public SoilResource(Item item, int maxCount)
        {
            MaxCount = maxCount;
            Count = 0;
            Item = item;
        }

        public int MaxCount { get; }
        public float Count { get; private set; }
        public Item Item { get; private set; }
        
        public bool IsFull()
        {
            return Count >= MaxCount;
        }
        
        public void Increment(float amount)
        {
            if (amount + Count > MaxCount)
            {
                Count = MaxCount;
            }
            else if (amount + Count < 0)
            {
                Count = 0;
            }
            else
            {
                Count += amount;
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
        private readonly Stack<SoilResource> _previousSoil = new();

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
            
            Item lastSoil = null;
            Item lastRecordedSoil = null;
            for (int i = 0; i < neededSoil; i++)
            {
                lastSoil = RemoveItem(soilData.IDHash);
                lastRecordedSoil = _previousSoil.Count > 0 ? _previousSoil.Pop().Item : null;
            }

            if (lastRecordedSoil != null)
            {
                _soil = new SoilResource(lastRecordedSoil, maxSoil);
                _soil.Increment(remainingAmount);
            }
            else if (lastSoil != null)
            {
                _soil = new SoilResource(lastSoil, maxSoil);
                _soil.Increment(remainingAmount);
            }
            _soil?.SetValue(remainingAmount);
            return true;
        }

        public SoilResource AddSoil(ItemData soilData, float amount)
        {
            _soil ??= new SoilResource(soilData.CreateInstance(),
                soilData.GetComponent<ResourceComponentData>().NeededCollectionCount);
            _soil.Increment(amount);

            if (!_soil.IsFull())
            {
                return _soil;
            }
            
            SoilResource result = _soil;
            if (!AddInternal(_soil.Item))
            {
                return null;
            }
            
            _previousSoil.Push(_soil);
            _soil = null;
            return result;
        }

        public IntermediateResource AddResource(ItemData item)
        {
            if (!_intermediateResources.TryGetValue(item.IDHash, out IntermediateResource intermediateResource))
            {
                intermediateResource = new IntermediateResource(item.CreateInstance());
                _intermediateResources[item.IDHash] = intermediateResource;
            }
            intermediateResource.Increment();

            if (!intermediateResource.IsFull())
            {
                return intermediateResource;
            }
            
            if (!AddInternal(intermediateResource.Item))
            {
                return null;
            }
            
            IntermediateResource result = intermediateResource;

            _intermediateResources.Remove(item.IDHash);
            return result;
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