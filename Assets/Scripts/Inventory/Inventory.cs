using System;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Inventory
{
    public static class InventoryConstants
    {
        public const int PlayerInventoryMaxWidth = 6;
        public const int PlayerInventoryMaxHeight = 9;
    }
    
    public interface IInventorySubscriber
    {
        public void OnInventoryChanged();
    }

    public interface IInventory
    {
        public List<Item> GetItems();
        public Item RemoveItem(int itemID);

        public void AddItem(Item item, Vector2Int position, int rotation);

        // Transfer items from this inventory to another inventory, restricted by a direction
        public void TransferItems(IInventory destination, TransferDirection direction);
        public string GetInventoryName();
    }

    public interface IInventoryNotifier
    {
        public void AddSubscriber(IInventorySubscriber subscriber);
        public void NotifySubscribersOnInventoryChanged();
    }

    [Flags]
    public enum TransferDirection
    {
        None = 0,
        SourceToDestination = 1,
        DestinationToSource = 2
    }
}