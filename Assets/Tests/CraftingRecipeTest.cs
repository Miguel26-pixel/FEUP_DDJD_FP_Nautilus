using System;
using System.Collections.Generic;
using System.Linq;
using Crafting;
using DataManager;
using Items;
using NUnit.Framework;

namespace Tests
{
    public class CraftingRecipeTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void CraftingRecipeCanCraft()
        {
            string testHash = 0.GetHashCode().ToString("X");
            string stuffHash = 1.GetHashCode().ToString("X");
            string resultHash = 2.GetHashCode().ToString("X");

            CraftingRecipe recipe = new(resultHash, MachineType.Assembler | MachineType.Smelter,
                new Dictionary<string, int> { { testHash, 1 }, { stuffHash, 2 } });

            ItemData test = new(testHash, "", "", ItemType.Consumable, "");
            ItemData stuff = new(stuffHash, "", "", ItemType.Consumable, "");

            Item testItem = test.CreateInstance();
            Item stuffItem = stuff.CreateInstance();

            Assert.IsFalse(recipe.CanCraft(MachineType.Assembler, new List<Item> { testItem }));
            Assert.IsFalse(recipe.CanCraft(MachineType.Assembler, new List<Item> { stuffItem }));
            Assert.IsFalse(recipe.CanCraft(MachineType.Assembler, new List<Item> { testItem, stuffItem }));
            Assert.IsTrue(recipe.CanCraft(MachineType.Assembler, new List<Item> { testItem, stuffItem, stuffItem }));
            Assert.IsTrue(recipe.CanCraft(MachineType.Assembler, new List<Item> { stuffItem, testItem, stuffItem }));
            Assert.IsFalse(recipe.CanCraft(MachineType.Fabricator, new List<Item> { stuffItem, testItem, stuffItem }));
            Assert.IsTrue(recipe.CanCraft(MachineType.Smelter, new List<Item> { stuffItem, testItem, stuffItem }));
            Assert.IsFalse(recipe.CanCraft(MachineType.Assembler, new List<Item> { stuffItem, stuffItem, stuffItem }));
            Assert.IsFalse(recipe.CanCraft(MachineType.Assembler, new List<Item> { testItem, testItem, testItem }));
            Assert.IsFalse(recipe.CanCraft(MachineType.Assembler, new List<Item>()));
        }

        [Test]
        public void CraftingRecipeSerialize()
        {
            string testHash = 0.GetHashCode().ToString("X");
            string stuffHash = 1.GetHashCode().ToString("X");
            string resultHash = 2.GetHashCode().ToString("X");

            CraftingRecipe recipe = new(resultHash, MachineType.Assembler | MachineType.Smelter,
                new Dictionary<string, int> { { testHash, 1 }, { stuffHash, 2 } }, 3);

            string json = DataSerializer.SerializeRecipeData(new List<CraftingRecipe> { recipe });

            MachineType machineType = MachineType.Assembler | MachineType.Smelter;
            object enumVal = Convert.ChangeType(machineType, machineType.GetTypeCode());
            Assert.AreEqual(
                $"[{{\"ingredients\":{{\"{testHash}\":1,\"{stuffHash}\":2}},\"result\":\"{resultHash}\"," +
                $"\"machineType\":{enumVal},\"quantity\":3}}]",
                json);
        }

        [Test]
        public void CraftingRecipeDeserialize()
        {
            string testHash = 0.GetHashCode().ToString("X");
            string stuffHash = 1.GetHashCode().ToString("X");
            string resultHash = 2.GetHashCode().ToString("X");
            MachineType machineType = MachineType.Assembler | MachineType.Smelter;
            object enumVal = Convert.ChangeType(machineType, machineType.GetTypeCode());

            string json = $"[{{\"ingredients\":{{\"{testHash}\":1,\"{stuffHash}\":2}},\"result\":\"{resultHash}\"," +
                          $"\"machineType\":{enumVal},\"quantity\":3}}]";

            CraftingRecipe[] recipes = DataDeserializer.DeserializeRecipeData(json).ToArray();

            Assert.AreEqual(1, recipes.Length);
            Assert.AreEqual(2, recipes[0].Ingredients.Count);
            Assert.AreEqual(1, recipes[0].Ingredients[0]);
            Assert.AreEqual(2, recipes[0].Ingredients[1]);
            Assert.AreEqual(2, recipes[0].Result);
            Assert.AreEqual(machineType, recipes[0].MachineType);
            Assert.AreEqual(3, recipes[0].Quantity);
        }
    }
}