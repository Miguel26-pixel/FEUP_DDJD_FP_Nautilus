using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Items;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class ItemGridTest
{
    [Test]
    public void ItemGridTestIcons()
    {
        ItemData itemData = new ("123", "Test Item", "This is a test", ItemType.Consumable, "ItemIcons/test-missing");
        
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
}
