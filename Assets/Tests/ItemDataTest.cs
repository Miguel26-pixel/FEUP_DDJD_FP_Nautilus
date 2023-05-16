using System;
using System.Collections.Generic;
using System.Linq;
using DataManager;
using Items;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class ItemDataTest
    {
        string singleCellGridJson;
        
        [SetUp]
        public void Setup()
        {
            singleCellGridJson = JsonConvert.SerializeObject(ItemGrid.SingleCellGrid());
        }
        
        [Test]
        public void ItemDataSerialize()
        {
            ItemData item = new("123", "Test Item", "This is a test", ItemType.Consumable, "ItemIcons/test");

            string ID = item.ID;
            string json = DataSerializer.SerializeItemData(new List<ItemData> { item });

            Assert.AreEqual(
                $"[{{\"id\":\"{ID}\",\"name\":\"Test Item\",\"description\":\"This is a test\"," +
                $"\"iconPath\":\"ItemIcons/test\",\"type\":3,\"components\":[],\"grid\":{singleCellGridJson}}}]",
                json);
        }

        [Test]
        public void ItemDataDeserialize()
        {
            string json = "[{\"id\":\"123\",\"name\":\"Test Item\",\"description\":\"This is a test\"," +
                          "\"iconPath\":\"ItemIcons/test\",\"type\":3,\"components\":[]}]";


            ItemData[] items = DataDeserializer.DeserializeItemData(json).ToArray();

            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("123", items[0].ID);
            Assert.AreEqual("Test Item", items[0].Name);
            Assert.AreEqual("This is a test", items[0].Description);
            Assert.AreEqual(ItemType.Consumable, items[0].Type);
            Assert.AreEqual(0, items[0].GetComponents().Count());
            Assert.AreEqual(ItemGrid.SingleCellGrid(), items[0].Grid);
        }

        [Test]
        public void ItemDataSerializeWithEquipment()
        {
            ItemData item = new("123", "Test Item", "This is a test", ItemType.Equipment, "ItemIcons/test");
            item.AddComponent(new EquipmentComponentData(
                0, 10, new List<Tuple<string, int>>
                {
                    new("test", 1),
                    new("test2", 3)
                })
            );

            string ID = item.ID;
            string json = DataSerializer.SerializeItemData(new List<ItemData> { item });

            Assert.AreEqual(
                $"[{{\"id\":\"{ID}\",\"name\":\"Test Item\",\"description\":\"This is a test\"," +
                "\"iconPath\":\"ItemIcons/test\",\"type\":5,\"components\":" +
                "[{\"$type\":\"Items.EquipmentComponentData, NautilusAssembly\"," +
                "\"enhancements\":[{\"Item1\":\"test\",\"Item2\":1},{\"Item1\":\"test2\",\"Item2\":3}]," +
                $"\"slot\":0,\"durability\":10}}],\"grid\":{singleCellGridJson}}}]",
                json);
        }

        [Test]
        public void ItemDataDeserializeWithEquipment()
        {
            string json = "[{\"id\":\"0\",\"name\":\"Test Item\",\"description\":\"This is a test\"," +
                          "\"iconPath\":\"ItemIcons/test\",\"type\":5,\"components\":" +
                          "[{\"$type\":\"Items.EquipmentComponentData, NautilusAssembly\"," +
                          "\"enhancements\":[{\"Item1\":\"test\",\"Item2\":1},{\"Item1\":\"test2\",\"Item2\":3}]," +
                          "\"slot\":0,\"durability\":10}]}]";

            ItemData[] items = DataDeserializer.DeserializeItemData(json).ToArray();

            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("0", items[0].ID);
            Assert.AreEqual("Test Item", items[0].Name);
            Assert.AreEqual("This is a test", items[0].Description);
            Assert.AreEqual(ItemType.Equipment, items[0].Type);
            Assert.AreEqual(1, items[0].GetComponents().Count());

            Assert.IsInstanceOf(typeof(EquipmentComponentData), items[0].GetComponents().First());
            EquipmentComponentData equipmentComponentData = (EquipmentComponentData)items[0].GetComponents().First();
            Assert.AreEqual(0, equipmentComponentData.Slot);
            Assert.AreEqual(10, equipmentComponentData.Durability);
            Assert.AreEqual(2, equipmentComponentData.Enhancements.Count);
            Assert.AreEqual("test", equipmentComponentData.Enhancements[0].Item1);
            Assert.AreEqual(1, equipmentComponentData.Enhancements[0].Item2);
            Assert.AreEqual("test2", equipmentComponentData.Enhancements[1].Item1);
            Assert.AreEqual(3, equipmentComponentData.Enhancements[1].Item2);
        }

        [Test]
        public void ItemDataSerializeWithConsumable()
        {
            ItemData item = new("123", "Test Item", "This is a test", ItemType.Consumable, "ItemIcons/test");
            item.AddComponent(new ConsumableComponentData(10, 5));

            string ID = item.ID;
            string json = DataSerializer.SerializeItemData(new List<ItemData> { item });

            Assert.AreEqual(
                $"[{{\"id\":\"{ID}\",\"name\":\"Test Item\",\"description\":\"This is a test\"," +
                "\"iconPath\":\"ItemIcons/test\",\"type\":3,\"components\":" +
                "[{\"$type\":\"Items.ConsumableComponentData, NautilusAssembly\"," +
                $"\"health\":10,\"hunger\":5}}],\"grid\":{singleCellGridJson}}}]",
                json);
        }

        [Test]
        public void ItemDataDeserializeWithConsumable()
        {
            string ID = "123";
            string json = $"[{{\"id\":\"{ID}\",\"name\":\"Test Item\",\"description\":\"This is a test\"," +
                          "\"iconPath\":\"ItemIcons/test\",\"type\":3,\"components\":" +
                          "[{\"$type\":\"Items.ConsumableComponentData, NautilusAssembly\"," +
                          "\"health\":10,\"hunger\":5}]}]";

            ItemData[] items = DataDeserializer.DeserializeItemData(json).ToArray();

            Assert.AreEqual(1, items.Length);
            Assert.AreEqual(ID, items[0].ID);
            Assert.AreEqual("Test Item", items[0].Name);
            Assert.AreEqual("This is a test", items[0].Description);
            Assert.AreEqual(ItemType.Consumable, items[0].Type);
            Assert.AreEqual(1, items[0].GetComponents().Count());

            Assert.IsInstanceOf(typeof(ConsumableComponentData), items[0].GetComponents().First());
            ConsumableComponentData consumableComponentData = (ConsumableComponentData)items[0].GetComponents().First();

            Assert.AreEqual(10, consumableComponentData.Health);
            Assert.AreEqual(5, consumableComponentData.Hunger);
        }

        [Test]
        public void ItemDataSerializeWithPlaceable()
        {
            Quaternion rotation = new(10, 20, 30, 5);
            ItemData item = new("123", "Test Item", "This is a test", ItemType.Machine, "ItemIcons/test");
            item.AddComponent(new PlaceableComponentData(
                    new SerializableGameObject(
                        "PlaceableObjects/Assembler",
                        new Vector3(0, 1, 2),
                        rotation)
                )
            );

            string ID = item.ID;
            string json = DataSerializer.SerializeItemData(new List<ItemData> { item });

            Assert.That(json, Is.EqualTo(
                    $"[{{\"id\":\"{ID}\",\"name\":\"Test Item\",\"description\":\"This is a test\"," +
                    "\"iconPath\":\"ItemIcons/test\",\"type\":6,\"components\":" +
                    "[{\"$type\":\"Items.PlaceableComponentData, NautilusAssembly\"," +
                    "\"placedGameObject\":{" +
                    "\"name\":\"PlaceableObjects/Assembler\"," +
                    "\"position\":{\"x\":0.0,\"y\":1.0,\"z\":2.0}," +
                    "\"rotation\":{\"x\":10.0,\"y\":20.0,\"z\":30.0,\"w\":5.0}" +
                    "}" +
                    $"}}],\"grid\":{singleCellGridJson}}}]"
                ).NoClip
            );
        }

        [Test]
        public void ItemDataDeserializeWithPlaceable()
        {
            string ID = "123";
            string json = $"[{{\"id\":\"{ID}\",\"name\":\"Test Item\",\"description\":\"This is a test\"," +
                          "\"iconPath\":\"ItemIcons/test\",\"type\":6,\"components\":" +
                          "[{\"$type\":\"Items.PlaceableComponentData, NautilusAssembly\"," +
                          "\"placedGameObject\":{" +
                          "\"name\":\"PlaceableObjects/Assembler\"," +
                          "\"position\":{\"x\":0.0,\"y\":1.0,\"z\":2.0}," +
                          "\"rotation\":{\"x\":10.0,\"y\":20.0,\"z\":30.0,\"w\":5.0}" +
                          "}" +
                          "}]}]";

            ItemData[] items = DataDeserializer.DeserializeItemData(json).ToArray();

            Assert.AreEqual(1, items.Length);
            Assert.AreEqual(ID, items[0].ID);
            Assert.AreEqual("Test Item", items[0].Name);
            Assert.AreEqual("This is a test", items[0].Description);
            Assert.AreEqual(ItemType.Machine, items[0].Type);
            Assert.AreEqual(1, items[0].GetComponents().Count());

            Assert.IsInstanceOf(typeof(PlaceableComponentData), items[0].GetComponents().First());
            PlaceableComponentData placeableComponentData = (PlaceableComponentData)items[0].GetComponents().First();

            Assert.AreEqual("PlaceableObjects/Assembler", placeableComponentData.PlacedGameObject.Name);
            Assert.AreEqual(new Vector3(0, 1, 2), placeableComponentData.PlacedGameObject.Position);
            Assert.AreEqual(new Quaternion(10, 20, 30, 5), placeableComponentData.PlacedGameObject.Rotation);
        }


        [Test]
        public void ItemDataSerializeWithTool()
        {
            ItemData item = new("123", "Test Item", "This is a test", ItemType.Tool, "ItemIcons/test");
            item.AddComponent(new ToolComponentData(
                    0,
                    10,
                    "Tool/test"
                )
            );

            string ID = item.ID;
            string json = DataSerializer.SerializeItemData(new List<ItemData> { item });

            Assert.That(json, Is.EqualTo(
                    $"[{{\"id\":\"{ID}\",\"name\":\"Test Item\",\"description\":\"This is a test\"," +
                    "\"iconPath\":\"ItemIcons/test\",\"type\":8,\"components\":" +
                    "[{\"$type\":\"Items.ToolComponentData, NautilusAssembly\"," +
                    "\"tool\":\"Tool/test\",\"slot\":0,\"durability\":10" +
                    $"}}],\"grid\":{singleCellGridJson}}}]"
                ).NoClip
            );
        }

        [Test]
        public void ItemDataDeserializeWithTool()
        {
            string ID = "123";
            string json = $"[{{\"id\":\"{ID}\",\"name\":\"Test Item\",\"description\":\"This is a test\"," +
                          "\"iconPath\":\"ItemIcons/test\",\"type\":8,\"components\":" +
                          "[{\"$type\":\"Items.ToolComponentData, NautilusAssembly\"," +
                          "\"tool\":\"Tool/test\",\"slot\":0,\"durability\":10" +
                          "}]}]";

            ItemData[] items = DataDeserializer.DeserializeItemData(json).ToArray();

            Assert.AreEqual(1, items.Length);
            Assert.AreEqual(ID, items[0].ID);
            Assert.AreEqual("Test Item", items[0].Name);
            Assert.AreEqual("This is a test", items[0].Description);
            Assert.AreEqual(ItemType.Tool, items[0].Type);
            Assert.AreEqual(1, items[0].GetComponents().Count());

            Assert.IsInstanceOf(typeof(ToolComponentData), items[0].GetComponents().First());
            ToolComponentData toolComponentData = (ToolComponentData)items[0].GetComponents().First();

            Assert.AreEqual("Tool/test", toolComponentData.Tool);
            Assert.AreEqual(0, toolComponentData.Slot);
            Assert.AreEqual(10, toolComponentData.Durability);
        }

        [Test]
        public void ItemDataSerializeWithWeapon()
        {
            ItemData item = new("123", "Test Item", "This is a test", ItemType.Weapon, "ItemIcons/test");
            item.AddComponent(new WeaponComponentData(
                    1,
                    12,
                    "Weapon/test"
                )
            );

            string ID = item.ID;
            string json = DataSerializer.SerializeItemData(new List<ItemData> { item });

            Assert.That(json, Is.EqualTo(
                    $"[{{\"id\":\"{ID}\",\"name\":\"Test Item\",\"description\":\"This is a test\"," +
                    "\"iconPath\":\"ItemIcons/test\",\"type\":4,\"components\":" +
                    "[{\"$type\":\"Items.WeaponComponentData, NautilusAssembly\"," +
                    "\"weapon\":\"Weapon/test\",\"slot\":1,\"durability\":12" +
                    $"}}],\"grid\":{singleCellGridJson}}}]"
                ).NoClip
            );
        }

        [Test]
        public void ItemDataDeserializeWithWeapon()
        {
            string ID = "123";
            string json = $"[{{\"id\":\"{ID}\",\"name\":\"Test Item\",\"description\":\"This is a test\"," +
                          "\"iconPath\":\"ItemIcons/test\",\"type\":4,\"components\":" +
                          "[{\"$type\":\"Items.WeaponComponentData, NautilusAssembly\"," +
                          "\"weapon\":\"Weapon/test\",\"slot\":1,\"durability\":12" +
                          "}]}]";

            ItemData[] items = DataDeserializer.DeserializeItemData(json).ToArray();

            Assert.AreEqual(1, items.Length);
            Assert.AreEqual(ID, items[0].ID);
            Assert.AreEqual("Test Item", items[0].Name);
            Assert.AreEqual("This is a test", items[0].Description);
            Assert.AreEqual(ItemType.Weapon, items[0].Type);
            Assert.AreEqual(1, items[0].GetComponents().Count());

            Assert.IsInstanceOf(typeof(WeaponComponentData), items[0].GetComponents().First());
            WeaponComponentData weaponComponentData = (WeaponComponentData)items[0].GetComponents().First();

            Assert.AreEqual("Weapon/test", weaponComponentData.Weapon);
            Assert.AreEqual(1, weaponComponentData.Slot);
            Assert.AreEqual(12, weaponComponentData.Durability);
        }
    }
}