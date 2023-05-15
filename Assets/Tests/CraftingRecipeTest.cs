using System.Collections;
using System.Collections.Generic;
using Crafting;
using Items;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CraftingRecipeTest
{
    // A Test behaves as an ordinary method
    [Test]
    public void CraftingRecipeCanCraft()
    {
        string testHash = 0.GetHashCode().ToString("X");
        string stuffHash = 1.GetHashCode().ToString("X");
        string resultHash = 2.GetHashCode().ToString("X");

        CraftingRecipe recipe = new CraftingRecipe(resultHash, MachineType.Assembler | MachineType.Smelter, new Dictionary<string, int> { { testHash, 1 }, {stuffHash, 2} });

        ItemData test = new ItemData(testHash, "", "", ItemType.Consumable, "");
        ItemData stuff = new ItemData(stuffHash, "", "", ItemType.Consumable, "");
       
        Assert.IsFalse(recipe.CanCraft(MachineType.Assembler, new List<ItemData> { test }));
        Assert.IsFalse(recipe.CanCraft(MachineType.Assembler, new List<ItemData> { stuff }));
        Assert.IsFalse(recipe.CanCraft(MachineType.Assembler, new List<ItemData> { test, stuff }));
        Assert.IsTrue(recipe.CanCraft(MachineType.Assembler, new List<ItemData> { test, stuff, stuff }));
        Assert.IsTrue(recipe.CanCraft(MachineType.Assembler, new List<ItemData> { stuff, test, stuff }));
        Assert.IsFalse(recipe.CanCraft(MachineType.Fabricator, new List<ItemData> { stuff, test, stuff }));
        Assert.IsTrue(recipe.CanCraft(MachineType.Smelter, new List<ItemData> { stuff, test, stuff }));
        Assert.IsFalse(recipe.CanCraft(MachineType.Assembler, new List<ItemData> {stuff, stuff, stuff}));
        Assert.IsFalse(recipe.CanCraft(MachineType.Assembler, new List<ItemData> {test, test, test}));
        Assert.IsFalse(recipe.CanCraft(MachineType.Assembler, new List<ItemData>()));
    }
}
