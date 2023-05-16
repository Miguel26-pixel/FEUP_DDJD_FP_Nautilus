using System;
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
        Assert.AreSame(item.Icons, itemData.Icons);

    }

    [Test]
    public void ItemSerializeWithEquipment()
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
        
        Item itemInstance = item.CreateInstance();
        EquipmentComponent component = itemInstance.GetComponent<EquipmentComponent>();
        component.CurrentDurability = 4;
        
        string json = DataSerializer.SerializeItem(itemInstance);
        
        Assert.That(json, Is.EqualTo(
                $"{{\"id\":\"{ID}\",\"components\":" +
                $"[{{\"$type\":\"Items.EquipmentComponent, NautilusAssembly\",\"currentDurability\":4}}]}}"
            ).NoClip
        );
    }

    [Test]
    public void ItemDeserializeWithEquipment()
    {
        ItemRegistry itemRegistry = new();
        
        ItemData itemData = itemRegistry.CreateItem("Test Item", "This is a test", ItemType.Equipment, "ItemIcons/test");
        var componentData = new EquipmentComponentData(
            0, 10, new List<Tuple<string, int>>
            {
                new("test", 1),
                new("test2", 3)
            });
        itemData.AddComponent(
            componentData
        );
        string ID = itemData.ID;

        string json = $"{{\"id\":\"{ID}\",\"components\":" +
                      $"[{{\"$type\":\"Items.EquipmentComponent, NautilusAssembly\",\"currentDurability\":4}}]}}";
        
        ItemConverter converter = new(itemRegistry);        
        Item item = DataDeserializer.DeserializeItem(json, converter);
        
        Assert.AreEqual(item.ID, ID);
        Assert.AreEqual(item.Name, "Test Item");
        Assert.AreEqual(item.Description, "This is a test");
        Assert.AreEqual(item.Type, ItemType.Equipment);
        Assert.AreSame(item.Icons, itemData.Icons);
        
        EquipmentComponent component = item.GetComponent<EquipmentComponent>();
        Assert.AreEqual(component.CurrentDurability, 4);
        Assert.AreSame(component.itemComponentData, componentData);
    }
    
    [Test]
    public void ItemSerializeWithPlaceable()
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
        
        Item itemInstance = item.CreateInstance();
        
        string json = DataSerializer.SerializeItem(itemInstance);
        
        Assert.That(json, Is.EqualTo(
                $"{{\"id\":\"{ID}\",\"components\":" +
                $"[{{\"$type\":\"Items.PlaceableComponent, NautilusAssembly\"}}]}}"
            ).NoClip
        );
    }

    [Test]
    public void ItemDeserializeWithPlaceable()
    {
        ItemRegistry itemRegistry = new();
        
        Quaternion rotation = new(10, 20, 30, 5);
        var componentData = new PlaceableComponentData(
            new SerializableGameObject(
                "PlaceableObjects/Assembler",
                new Vector3(0, 1, 2),
                rotation)
        );
        ItemData itemData = itemRegistry.CreateItem("Test Item", "This is a test", ItemType.Machine, "ItemIcons/test");
        itemData.AddComponent(
            componentData
        );
        string ID = itemData.ID;

        string json = $"{{\"id\":\"{ID}\",\"components\":" +
                      $"[{{\"$type\":\"Items.PlaceableComponent, NautilusAssembly\"}}]}}";
        
        ItemConverter converter = new(itemRegistry);        
        Item item = DataDeserializer.DeserializeItem(json, converter);
        
        Assert.AreEqual(item.ID, ID);
        Assert.AreEqual(item.Name, "Test Item");
        Assert.AreEqual(item.Description, "This is a test");
        Assert.AreEqual(item.Type, ItemType.Machine);
        Assert.AreSame(item.Icons, itemData.Icons);

        
        PlaceableComponent component = item.GetComponent<PlaceableComponent>();
        Assert.AreSame(component.itemComponentData, componentData);
    }
}
