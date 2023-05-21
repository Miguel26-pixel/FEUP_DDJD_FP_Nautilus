using System;
using System.Collections.Generic;
using Items;

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
        public void AddItem(Item item);
        public Item RemoveItem(int itemID);
        public string GetInventoryName();
    }

    public interface IInventoryNotifier
    {
        public void AddSubscriber(IInventorySubscriber subscriber);
        public void NotifySubscribersOnInventoryChanged();
    }

    [Serializable]
    [Flags]
    public enum TransferDirection
    {
        None = 1,
        SourceToDestination = 2,
        DestinationToSource = 4
    }
}