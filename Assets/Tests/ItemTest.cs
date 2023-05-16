using System.Collections.Generic;
using System.Linq;
using DataManager;
using Items;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class ItemTest
    {
        [Test]
        public void ItemDataSerialize()
        {
            ItemData item = new("123", "Test Item", "This is a test", ItemType.Consumable, "ItemIcons/test");

            string ID = item.ID;
            string json = DataSerializer.SerializeItemData(new List<ItemData> { item });

            Assert.AreEqual(
                $"[{{\"id\":\"{ID}\",\"name\":\"Test Item\",\"description\":\"This is a test\",\"iconPath\":\"ItemIcons/test\",\"type\":3,\"components\":[]}}]",
                json);
        }

        [Test]
        public void ItemDataDeserialize()
        {
            Sprite sprite = Resources.Load<Sprite>("ItemIcons/test"); 
            string json = $"[{{\"id\":\"123\",\"name\":\"Test Item\",\"description\":\"This is a test\"," +
                          $"\"iconPath\":\"ItemIcons/test\",\"type\":3,\"components\":[]}}]";


            ItemData[] items = DataDeserializer.DeserializeItemData(json).ToArray();

            Assert.AreEqual(1, items.Length);
            Assert.AreEqual("123", items[0].ID);
            Assert.AreEqual("Test Item", items[0].Name);
            Assert.AreEqual("This is a test", items[0].Description);
            Assert.AreEqual(sprite, items[0].Icon);
            Assert.AreEqual(ItemType.Consumable, items[0].Type);
            Assert.AreEqual(0, items[0].GetComponents().Count());
        }
    }
}