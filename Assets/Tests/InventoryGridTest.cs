using System;
using System.Collections;
using Inventory;
using Items;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class InventoryGridTest
    {
        private InventoryGrid _inventoryGrid;
        private bool[,] _gridShape;

        public Item Get2x2Item()
        {
            return new ItemData(
                "123",
                "2x2 Item",
                "2x2 Item Description",
                ItemType.Equipment,
                "ItemIcons/test",
                new bool[,]
                {
                    { true, true },
                    { true, true }
                }
            ).CreateInstance();
        }

        public Item GetLItem()
        {
            return new ItemData(
                "123",
                "L Item",
                "L Item Description",
                ItemType.Equipment,
                "ItemIcons/test",
                new bool[,]
                {
                    { true, false },
                    { true, false },
                    { true, true }
                }
            ).CreateInstance();
        }
        
        public Item Get2x1Item()
        {
            return new ItemData(
                "123",
                "2x1 Item",
                "2x1 Item Description",
                ItemType.Equipment,
                "ItemIcons/test",
                new bool[,]
                {
                    { true },
                    { true }
                }
            ).CreateInstance();
        }

        public Item Get1x2Item()
        {
            return new ItemData(
                "123",
                "1x2 Item",
                "1x2 Item Description",
                ItemType.Equipment,
                "ItemIcons/test",
                new bool[,]
                {
                    { true, true }
                }
            ).CreateInstance();
        }
        
        public Item GetOffset1x2Item()
        {
            return new ItemData(
                "123",
                "Offset 2x1 Item",
                "Offset 2x1 Item Description",
                ItemType.Equipment,
                "ItemIcons/test",
                new bool[,]
                {
                    { false, false, false },
                    { false, true, true }
                }
            ).CreateInstance();
        }
        
        [SetUp]
        public void SetUp()
        {
            _gridShape = new bool[,]
            {
                {false, true, true, false},
                {true, true, true, true},
                {false, true, true, false},
            };
            
            _inventoryGrid = new InventoryGrid(_gridShape);
        }

        [Test]
        public void AddItemTest()
        {
            var item = Get2x2Item();
            var position = new Vector2Int(1, 0);
            
            _inventoryGrid.AddItem(item, position, 0);
            
            var returnedItem = _inventoryGrid.GetAt(position);
            Assert.AreEqual(item, returnedItem);
            returnedItem = _inventoryGrid.GetAt(position + Vector2Int.up);
            Assert.AreEqual(item, returnedItem);
            returnedItem = _inventoryGrid.GetAt(position + Vector2Int.right);
            Assert.AreEqual(item, returnedItem);
            returnedItem = _inventoryGrid.GetAt(position + Vector2Int.one);
            Assert.AreEqual(item, returnedItem);
            Assert.Throws<InvalidItemPositionException>(() => _inventoryGrid.GetAt(position + Vector2Int.left));
            Assert.Throws<InvalidItemPositionException>(() => _inventoryGrid.GetAt(position + Vector2Int.right * 2));
            returnedItem = _inventoryGrid.GetAt(position + Vector2Int.up + Vector2Int.right * 2);
            Assert.IsNull(returnedItem);
        }

        [Test]
        public void AddItemInvalidPositionOutOfBounds()
        {
            var item = Get2x2Item();
            
            var position = new Vector2Int(3, 0);
            Assert.Throws<InvalidItemPositionException>(() => _inventoryGrid.AddItem(item, position, 0));
            position = new Vector2Int(1, 3);
            Assert.Throws<InvalidItemPositionException>(() => _inventoryGrid.AddItem(item, position, 0));
            position = new Vector2Int(-1, 1);
            Assert.Throws<InvalidItemPositionException>(() => _inventoryGrid.AddItem(item, position, 0));
            position = new Vector2Int(1, -1);
            Assert.Throws<InvalidItemPositionException>(() => _inventoryGrid.AddItem(item, position, 0));
            position = new Vector2Int(0, 0);
            Assert.Throws<InvalidItemPositionException>(() => _inventoryGrid.AddItem(item, position, 0));
        }

        [Test]
        public void AddItemInvalidPositionDoesNotFit()
        {
            var item = Get2x2Item();
            
            var position = new Vector2Int(2, 2);
            Assert.Throws<ItemDoesNotFitException>(() => _inventoryGrid.AddItem(item, position, 0));
            
            position = new Vector2Int(3, 1);
            Assert.Throws<ItemDoesNotFitException>(() => _inventoryGrid.AddItem(item, position, 0));
            
            position = new Vector2Int(2, 0);
            Assert.Throws<ItemDoesNotFitException>(() => _inventoryGrid.AddItem(item, position, 0));
            
            position = new Vector2Int(1, 2);
            Assert.Throws<ItemDoesNotFitException>(() => _inventoryGrid.AddItem(item, position, 0));
        }

        [Test]
        public void AddItemInvalidPositionOccupied()
        {
            var item = Get2x2Item();
            var position = new Vector2Int(1, 1);
            
            _inventoryGrid.AddItem(item, position, 0);
            
            Assert.Throws<PositionAlreadyOccupiedException>(() => _inventoryGrid.AddItem(item, position, 0));
            
            var item2 = Get2x1Item();
            
            Assert.Throws<PositionAlreadyOccupiedException>(() => _inventoryGrid.AddItem(item2, position, 0));

            position = new Vector2Int(1, 0);
            Assert.Throws<PositionAlreadyOccupiedException>(() => _inventoryGrid.AddItem(item2, position, 0));

            var item3 = Get1x2Item();
            
            position = new Vector2Int(0, 1);
            Assert.Throws<PositionAlreadyOccupiedException>(() => _inventoryGrid.AddItem(item3, position, 0));
            
            position = new Vector2Int(2, 1);
            Assert.Throws<PositionAlreadyOccupiedException>(() => _inventoryGrid.AddItem(item3, position, 0));
        }

        [Test]
        public void AddItemOffsetSuccess()
        {
            var item = GetOffset1x2Item();
            var position = new Vector2Int(1, 0);
            _inventoryGrid.AddItem(item, position, 0);
            
            var returnedItem = _inventoryGrid.GetAt(position);
            Assert.AreEqual(item, returnedItem);
            returnedItem = _inventoryGrid.GetAt(position + Vector2Int.right);
            Assert.AreEqual(item, returnedItem);
            returnedItem = _inventoryGrid.GetAt(position + Vector2Int.up);
            Assert.IsNull(returnedItem);
        }
        
        [Test]
        public void AddItemOffsetInvalidPositionDoesNotFit()
        {
            var item = GetOffset1x2Item();
            
            var position = new Vector2Int(2, 2);
            Assert.Throws<ItemDoesNotFitException>(() => _inventoryGrid.AddItem(item, position, 0));
            
            position = new Vector2Int(3, 1);
            Assert.Throws<ItemDoesNotFitException>(() => _inventoryGrid.AddItem(item, position, 0));
        }
    }
}
