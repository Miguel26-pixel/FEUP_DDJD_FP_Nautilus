using System;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Player
{
    public class InventoryMock : IInventory
    {
        public List<Item> items = new List<Item>();

        public List<Item> GetItems()
        {
            return items;
        }
    }
}