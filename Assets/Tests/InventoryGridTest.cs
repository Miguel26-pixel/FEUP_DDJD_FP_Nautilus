using System;
using System.Collections;
using Inventory;
using Items;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Utils;

namespace Tests
{
    public class InventoryGridTest
    {
        private InventoryGrid _inventoryGrid;
        private bool[,] _gridShape;

        private static Item Get2X2Item()
        {
            return new ItemData(
                "22",
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

        private static Item GetLItem()
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
        
        public static Item Get2X1Item()
        {
            return new ItemData(
                "21",
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

        public static Item Get1X2Item()
        {
            return new ItemData(
                "12",
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
        
        public static Item GetOffset1X2Item()
        {
            return new ItemData(
                "offset",
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
            var item = Get2X2Item();
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
            var item = Get2X2Item();
            
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
            var item = Get2X2Item();
            
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
            var item = Get2X2Item();
            var position = new Vector2Int(1, 1);
            
            _inventoryGrid.AddItem(item, position, 0);
            
            Assert.Throws<PositionAlreadyOccupiedException>(() => _inventoryGrid.AddItem(item, position, 0));
            
            var item2 = Get2X1Item();
            
            Assert.Throws<PositionAlreadyOccupiedException>(() => _inventoryGrid.AddItem(item2, position, 0));

            position = new Vector2Int(1, 0);
            Assert.Throws<PositionAlreadyOccupiedException>(() => _inventoryGrid.AddItem(item2, position, 0));

            var item3 = Get1X2Item();
            
            position = new Vector2Int(0, 1);
            Assert.Throws<PositionAlreadyOccupiedException>(() => _inventoryGrid.AddItem(item3, position, 0));
            
            position = new Vector2Int(2, 1);
            Assert.Throws<PositionAlreadyOccupiedException>(() => _inventoryGrid.AddItem(item3, position, 0));
        }

        [Test]
        public void AddItemOffsetSuccess()
        {
            var item = GetOffset1X2Item();
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
            var item = GetOffset1X2Item();
            
            var position = new Vector2Int(2, 2);
            Assert.Throws<ItemDoesNotFitException>(() => _inventoryGrid.AddItem(item, position, 0));
            
            position = new Vector2Int(3, 1);
            Assert.Throws<ItemDoesNotFitException>(() => _inventoryGrid.AddItem(item, position, 0));
        }

        [Test]
        public void RemoveItemSuccess()
        {
            var item = Get2X2Item();
            var position = new Vector2Int(1, 0);
            _inventoryGrid.AddItem(item, position, 0);
            
            var result = _inventoryGrid.RemoveItem(item.IDHash);
            Assert.AreSame(item, result);
            
            var returnedItem = _inventoryGrid.GetAt(position);
            Assert.IsNull(returnedItem);
        }
        
        [Test]
        public void RemoveItemInvalidItem()
        {
            var item = Get2X2Item();
            var position = new Vector2Int(1, 0);
            _inventoryGrid.AddItem(item, position, 0);
            
            var item2 = Get1X2Item();
            Assert.Throws<ItemNotInInventoryException>(() => _inventoryGrid.RemoveItem(item2.IDHash));
        }
        
        [Test]
        public void RemoveAtSuccess()
        {
            var item = Get2X2Item();
            var position = new Vector2Int(1, 0);
            _inventoryGrid.AddItem(item, position, 0);
            
            var result = _inventoryGrid.RemoveAt(position);
            Assert.AreSame(item, result);
            
            var returnedItem = _inventoryGrid.GetAt(position);
            Assert.IsNull(returnedItem);
        }

        [Test]
        public void RemoveAtSuccessDifferentPosition()
        {
            var item = Get2X2Item();
            var position = new Vector2Int(1, 0);
            _inventoryGrid.AddItem(item, position, 0);
            
            var result = _inventoryGrid.RemoveAt(position + Vector2Int.right);
            Assert.AreSame(item, result);
            
            var returnedItem = _inventoryGrid.GetAt(position);
            Assert.IsNull(returnedItem);
        }

        [Test]
        public void RemoveAtConcurrent()
        {
            var itemL = GetLItem();
            var item2x1 = Get2X1Item();
            
            var positionL = new Vector2Int(1, 0);
            var position2x1 = new Vector2Int(2, 0);
            
            _inventoryGrid.AddItem(item2x1, position2x1, 0);
            _inventoryGrid.AddItem(itemL, positionL, 0);

            var result = _inventoryGrid.RemoveAt(positionL);
            Assert.AreSame(itemL, result);
            
            var returnedItem = _inventoryGrid.GetAt(positionL);
            Assert.IsNull(returnedItem);
            
            returnedItem = _inventoryGrid.GetAt(position2x1);
            Assert.AreSame(item2x1, returnedItem);
        }

        [Test]
        public void RemoveAtNoItemPosition()
        {
            var nonPosition = new Vector2Int(1, 0);
            
            Assert.Throws<ItemNotInInventoryPositionException>(() => _inventoryGrid.RemoveAt(nonPosition));
        }

        [Test]
        public void ModuloRotation()
        {
            var rotation = MathUtils.Modulo(0 + 2, 4) - 2;
            Assert.AreEqual(0, rotation);
            rotation = MathUtils.Modulo(1 + 2, 4) - 2;
            Assert.AreEqual(1, rotation);
            rotation = MathUtils.Modulo(2 + 2, 4) - 2;
            Assert.AreEqual(-2, rotation);
            rotation = MathUtils.Modulo(3 + 2, 4) - 2;
            Assert.AreEqual(-1, rotation);
            rotation = MathUtils.Modulo(4 + 2, 4) - 2;
            Assert.AreEqual(0, rotation);
            rotation = MathUtils.Modulo(-1 + 2, 4) - 2;
            Assert.AreEqual(-1, rotation);
            rotation = MathUtils.Modulo(-2 + 2, 4) - 2;
            Assert.AreEqual(-2, rotation);
            rotation = MathUtils.Modulo(-3 + 2, 4) - 2;
            Assert.AreEqual(1, rotation);
            rotation = MathUtils.Modulo(-4 + 2, 4) - 2;
            Assert.AreEqual(0, rotation);
        }
        
        [Test]
        public void PlaceRotatedItemSuccess()
        {
            var item = Get2X1Item();
            var position = new Vector2Int(0, 1);
            
            Assert.Throws<ItemDoesNotFitException>(() => _inventoryGrid.AddItem(item, position, 0));
            
            _inventoryGrid.AddItem(item, position, 1);
            var returnedItem = _inventoryGrid.GetAt(position);
            Assert.AreSame(item, returnedItem);

            returnedItem = _inventoryGrid.GetAt(position + Vector2Int.right);
            Assert.AreSame(item, returnedItem);
        }

        [Test]
        public void PlaceRotatedItemFail()
        {
            var item = Get2X1Item();
            var position = new Vector2Int(0, 1);
            
            Assert.Throws<ItemDoesNotFitException>(() => _inventoryGrid.AddItem(item, position, 0));
            Assert.Throws<ItemDoesNotFitException>(() => _inventoryGrid.AddItem(item, position, 2));
        }

        [Test]
        public void PlaceRotatedL()
        {
            var item = GetLItem();
            var position = new Vector2Int(1, 1);
            
            Assert.Throws<ItemDoesNotFitException>(() => _inventoryGrid.AddItem(item, position, 0));
            
            _inventoryGrid.AddItem(item, position, -1);
            var returnedItem = _inventoryGrid.GetAt(position);
            Assert.AreSame(item, returnedItem);
        }
    }
}
