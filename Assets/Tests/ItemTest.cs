using System.Collections;
using System.Collections.Generic;
using DataManager;
using Items;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ItemTest
{
    [Test]
    public void ItemSerializeSimpleTest()
    {
        ItemData itemData = new("123", "Test Item", "This is a test", ItemType.Consumable, "ItemIcons/test");
        string ID = itemData.ID;

        Item item = itemData.CreateInstance();
        
        string json = DataSerializer.SerializeItem(item);
        
        Assert.That(json, Is.EqualTo(
                $"{{\"id\":\"{ID}\",\"components\":[]}}"
            ).NoClip
        );
    }

    [Test]
    public void ItemDeserializeSimpleTest()
    {
        ItemRegistry itemRegistry = new();
        ItemData itemData = itemRegistry.CreateItem("Test Item", "This is a test", ItemType.Consumable, "ItemIcons/test");
        string ID = itemData.ID;

        string json = $"{{\"id\":\"{ID}\",\"components\":[]}}";
        
        ItemConverter converter = new(itemRegistry);        
        Item item = DataDeserializer.DeserializeItem(json, converter);
        
        Assert.That(item.ID, Is.EqualTo(ID));
        Assert.That(item.GetComponents(), Is.Empty);
        Assert.AreEqual(item.Name, "Test Item");
        Assert.AreEqual(item.Description, "This is a test");
        Assert.AreEqual(item.Type, ItemType.Consumable);
        Assert.AreEqual(item.Icon, itemData.Icon);
    }
}
