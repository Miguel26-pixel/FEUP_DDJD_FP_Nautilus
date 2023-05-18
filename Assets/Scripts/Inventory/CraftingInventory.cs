using System;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Inventory
{
    public class CraftingInventory : IInventory
    {
        private readonly List<Item> _items;

        public CraftingInventory(List<Item> items)
        {
            _items = items;
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

                    return item;
                }
            }

            return null;
        }

        public void AddItem(Item item, Vector2Int position, int rotation)
        {
            throw new NotImplementedException();
        }

        public void TransferItems(IInventory destination, TransferDirection direction)
        {
            throw new NotImplementedException();
        }

        public string GetInventoryName()
        {
            return "Crafting Result";
        }
    }
}