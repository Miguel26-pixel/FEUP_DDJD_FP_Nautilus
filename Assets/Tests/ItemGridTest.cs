using System.Collections.Generic;
using System.Linq;
using DataManager;
using Items;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class ItemGridTest
    {
        [Test]
        public void ItemGridIconsTest()
        {
            ItemData itemData = new (
                "123",
                "Test Item",
                "This is a test",
                ItemType.Consumable,
                "ItemIcons/test-missing",
                new bool[,]
                {
                    {true, true, true, true},
                    {true, true, true, true},
                    {true, true, true, true},
                    {true, true, true, true}
                });
        
            List<Sprite> spriteSheet = Resources.LoadAll<Sprite>("ItemIcons/test-missing").ToList();

            // only 0_0, 2_1, and 3_3 should be null
            Assert.That(spriteSheet, Is.Not.Empty);
            Assert.AreEqual(spriteSheet.Count, 13);
            Assert.AreEqual(null, itemData.Icons[0, 0]);
            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_0_1"), itemData.Icons[0, 1]);
            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_0_2"), itemData.Icons[0, 2]);
            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_0_3"), itemData.Icons[0, 3]);
            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_1_0"), itemData.Icons[1, 0]);
            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_1_1"), itemData.Icons[1, 1]);
            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_1_2"), itemData.Icons[1, 2]);
            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_1_3"), itemData.Icons[1, 3]);
            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_2_0"), itemData.Icons[2, 0]);
            Assert.AreEqual(null, itemData.Icons[2, 1]);
            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_2_2"), itemData.Icons[2, 2]);
            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_2_3"), itemData.Icons[2, 3]);
            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_3_0"), itemData.Icons[3, 0]);
            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_3_1"), itemData.Icons[3, 1]);
            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_3_2"), itemData.Icons[3, 2]);
            Assert.AreEqual(null, itemData.Icons[3, 3]);
        }

        [Test]
        public void ItemGridIconsNotFull()
        {
            ItemData itemData = new (
                "123",
                "Test Item",
                "This is a test",
                ItemType.Consumable,
                "ItemIcons/test",
                new bool[,]
                {
                    {true, true, true, true},
                    {true, false, false, false},
                    {false, false, false, false},
                    {true, false, false, false}
                });
        
            List<Sprite> spriteSheet = Resources.LoadAll<Sprite>("ItemIcons/test").ToList();

            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_0_0"), itemData.Icons[0, 0]);
            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_0_1"), itemData.Icons[0, 1]);
            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_0_2"), itemData.Icons[0, 2]);
            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_0_3"), itemData.Icons[0, 3]);
            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_1_0"), itemData.Icons[1, 0]);
            Assert.AreEqual(null, itemData.Icons[1, 1]);
            Assert.AreEqual(null, itemData.Icons[1, 2]);
            Assert.AreEqual(null, itemData.Icons[1, 3]);
            Assert.AreEqual(null, itemData.Icons[2, 0]);
            Assert.AreEqual(null, itemData.Icons[2, 1]);
            Assert.AreEqual(null, itemData.Icons[2, 2]);
            Assert.AreEqual(null, itemData.Icons[2, 3]);
            Assert.AreEqual(spriteSheet.FirstOrDefault(sprite => sprite.name == "test_3_0"), itemData.Icons[3, 0]);
            Assert.AreEqual(null, itemData.Icons[3, 1]);
            Assert.AreEqual(null, itemData.Icons[3, 2]);
            Assert.AreEqual(null, itemData.Icons[3, 3]);
        }

        [Test]
        public void ItemGridSerializeTest()
        {
            ItemData itemData = new (
                "123",
                "Test Item",
                "This is a test",
                ItemType.Consumable,
                "ItemIcons/test",
                new bool[,]
                {
                    {true, true, true, true},
                    {true, false, false, false},
                    {false, false, false, false},
                    {true, false, false, false}
                });
            
            string json = DataSerializer.SerializeItemData(new List<ItemData> { itemData });
            
            Assert.AreEqual(
                $"[{{\"id\":\"123\",\"name\":\"Test Item\",\"description\":\"This is a test\"," +
                $"\"iconPath\":\"ItemIcons/test\",\"type\":3,\"components\":[],\"grid\":[" +
                $"[true,true,true,true]," +
                $"[true,false,false,false]," +
                $"[false,false,false,false]," +
                $"[true,false,false,false]" +
                "]}]",
                json);
        }

        [Test]
        public void ItemGridDeserializeTest()
        {
            string json = $"[{{\"id\":\"123\",\"name\":\"Test Item\",\"description\":\"This is a test\"," +
                          $"\"iconPath\":\"ItemIcons/test\",\"type\":3,\"components\":[],\"grid\":[" +
                          $"[true,true,true,true]," +
                          $"[true,false,false,false]," +
                          $"[false,false,false,false]," +
                          $"[true,false,false,false]" +
                          "]}]";
            
            List<ItemData> itemData = DataDeserializer.DeserializeItemData(json).ToList();
            
            Debug.Log(itemData[0].Grid);
            
            Assert.AreEqual(1, itemData.Count);
            Assert.AreEqual("123", itemData[0].ID);
            Assert.AreEqual("Test Item", itemData[0].Name);
            Assert.AreEqual("This is a test", itemData[0].Description);
            Assert.AreEqual(ItemType.Consumable, itemData[0].Type);
            Assert.AreEqual(new bool[,]
            {
                {true, true, true, true},
                {true, false, false, false},
                {false, false, false, false},
                {true, false, false, false}
            }, itemData[0].Grid);
            
        }


        [Test]
        public void ItemGridRotateClockwise()
        {
            bool[,] grid = new bool[,]
            {
                { true, true, true, true },
                { false, true, true, false },
                { false, true, false, false },
                { false, true, false, false}
            };
            
            bool[,] rotatedGrid = new bool[,]
            {
                { false, false, false, true },
                { true, true, true, true },
                { false, false, true, true },
                { false, false, false, true }
            };
            
            
            Assert.AreEqual(rotatedGrid, ItemGrid.RotateClockwise(grid));
        }

        [Test]
        public void ItemGridRotateCounterClockwise()
        {
            bool[,] grid = new bool[,]
            {
                { true, true, true, true },
                { false, true, true, false },
                { false, true, false, false },
                { false, true, false, false}
            };

            
            bool[,] rotatedGrid = new bool[,]
            {
                { true, false, false, false },
                { true, true, false, false },
                { true, true, true, true },
                { true, false, false, false }
            };
            
            Assert.AreEqual(rotatedGrid, ItemGrid.RotateCounterClockwise(grid));
        }

        [Test]
        public void ItemGridRotateTwice()
        {
            bool[,] grid = new bool[,]
            {
                { true, true, true, true },
                { false, true, true, false },
                { false, true, false, false },
                { false, true, false, false}
            };

            bool[,] rotatedGrid = new bool[,]
            {
                { false, false, true, false },
                { false, false, true, false },
                { false, true, true, false },
                { true, true, true, true}
            };
            
            Assert.AreEqual(rotatedGrid, ItemGrid.RotateClockwise(ItemGrid.RotateClockwise(grid)));
            Assert.AreEqual(rotatedGrid, ItemGrid.RotateCounterClockwise(ItemGrid.RotateCounterClockwise(grid)));
        }
    }
}
