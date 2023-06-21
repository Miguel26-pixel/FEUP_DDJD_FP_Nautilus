using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Crafting;
using Items;
using Newtonsoft.Json;
using UI.Inventory.Components;
using UnityEngine;

namespace DataManager
{
    public static class DataSerializer
    {
        public static string SerializeItemData(IEnumerable<ItemData> items)
        {
            return JsonConvert.SerializeObject(items,
                new JsonSerializerSettings
                    { TypeNameHandling = TypeNameHandling.Auto, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        public static string SerializeItem(Item item)
        {
            return JsonConvert.SerializeObject(item,
                new JsonSerializerSettings
                    { TypeNameHandling = TypeNameHandling.Auto, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        public static string SerializeRecipeData(IEnumerable<CraftingRecipe> recipes)
        {
            return JsonConvert.SerializeObject(recipes,
                new JsonSerializerSettings
                    { TypeNameHandling = TypeNameHandling.Auto, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }

    /// <summary>
    ///     Used only once to generate the ItemData.json file.
    ///     This file is then used to load items into the game.
    ///     Hence, this script is not needed in the final build.
    /// </summary>
    public class DataGenerator : MonoBehaviour
    {
        public ItemRegistryObject itemRegistryObject;
        public CraftingRecipeRegistryObject recipeRegistryObject;
        private ItemRegistry _itemRegistry;
        private CraftingRecipeRegistry _recipeRegistry;

        private void Start()
        {
            _itemRegistry = itemRegistryObject.itemRegistry;
            _recipeRegistry = recipeRegistryObject.craftingRecipeRegistry;

            CreateItems();
            Debug.Log("Done!");
        }

        private void CreateItems()
        {
            // Simple resources from gathering and monsters, without any components.
            ItemData fiber = _itemRegistry.CreateItem("Fiber", "Tangled treasures from island grass.",
                ItemType.Resource,
                "ItemIcons/test");
            ResourceComponentData resourceComponentData = new(8);
            fiber.AddComponent(resourceComponentData);

            ItemData wood = _itemRegistry.CreateItem("Wood",
                "Sun-kissed timber harvested from majestic palm trees." +
                " Ideal for crafting and roasting marshmallows (if you can find any).",
                ItemType.Resource, "ItemIcons/test", new[,]
                {
                    { true, true }
                });
            ResourceComponentData woodResourceComponentData = new(1);
            wood.AddComponent(woodResourceComponentData);

            ItemData seaweed = _itemRegistry.CreateItem("Seaweed Fiber", "Seashore secret for crafting needs.",
                ItemType.Resource,
                "ItemIcons/test");
            ResourceComponentData seaweedResourceComponentData = new(8);
            seaweed.AddComponent(seaweedResourceComponentData);

            ItemData calcium = _itemRegistry.CreateItem("Seashell Calcium", "Calcium-rich treasures from seashells.",
                ItemType.Resource,
                "ItemIcons/test");
            ResourceComponentData calciumResourceComponentData = new(3);
            calcium.AddComponent(calciumResourceComponentData);

            ItemData scales = _itemRegistry.CreateItem("Fish Scales", "Gleaming scales from the planet’s fishes.",
                ItemType.CreatureDrop,
                "ItemIcons/test");
            ItemData coralium = _itemRegistry.CreateItem("Coralium", "Vibrant oceanic resource harvested from corals.",
                ItemType.Resource,
                "ItemIcons/test");
            ResourceComponentData coraliumResourceComponentData = new(6);
            coralium.AddComponent(coraliumResourceComponentData);

            ItemData abyssallite = _itemRegistry.CreateItem("Abyssallite",
                "Deep-sea gem, hidden in the depths. A rare find for adventurous divers.", ItemType.Resource,
                "ItemIcons/test");
            ResourceComponentData abyssalliteResourceComponentData = new(5);
            abyssallite.AddComponent(abyssalliteResourceComponentData);

            ItemData kelp = _itemRegistry.CreateItem("Kelp Iron", "Iron with a touch of kelp magic.", ItemType.Resource,
                "ItemIcons/test");
            ResourceComponentData kelpResourceComponentData = new(5);
            kelp.AddComponent(kelpResourceComponentData);

            ItemData spines = _itemRegistry.CreateItem("Sea Urchin Spines", "Prickly prizes from deep-sea urchins.",
                ItemType.CreatureDrop,
                "ItemIcons/test");
            ItemData skin = _itemRegistry.CreateItem("Leviathan Skin", "A testament to conquering the sea's mightiest.",
                ItemType.CreatureDrop, "ItemIcons/test");
            ItemData tooth = _itemRegistry.CreateItem("Leviathan Tooth",
                "Enormous tooth of the legendary Leviathan. Handle carefully, display proudly.", ItemType.CreatureDrop,
                "ItemIcons/test");
            ItemData tear = _itemRegistry.CreateItem("Crystalline Tear", "Glistening tear shed by the Blackened Eye.",
                ItemType.CreatureDrop, "ItemIcons/test");
            ItemData tentacle = _itemRegistry.CreateItem("Deep Tentacle",
                "Sinister tentacle harvested from the dreaded Devourer.",
                ItemType.CreatureDrop, "ItemIcons/test");
            ItemData lumini = _itemRegistry.CreateItem("Luminitite", "Glowing mineral illuminating the caves.",
                ItemType.Resource,
                "ItemIcons/test");
            ResourceComponentData luminiResourceComponentData = new(5);
            lumini.AddComponent(luminiResourceComponentData);

            ItemData doomstone = _itemRegistry.CreateItem("Doomstone",
                "Foreboding stone found in the depths of the island's caves.",
                ItemType.Resource, "ItemIcons/test");
            ResourceComponentData doomstoneResourceComponentData = new(5);
            doomstone.AddComponent(doomstoneResourceComponentData);

            ItemData eye = _itemRegistry.CreateItem("Dreadworm Eye",
                "Eerie eye, snatched from the fearsome Dreadworm. Peer into its unsettling gaze.",
                ItemType.CreatureDrop, "ItemIcons/test");
            ItemData artifact = _itemRegistry.CreateItem("Alien Artifact",
                "From the depths of the cave comes this mysterious artifact." +
                " Is it a remnant of an ancient civilization or something else?",
                ItemType.Resource, "ItemIcons/test");
            ResourceComponentData artifactResourceComponentData = new(1);
            artifact.AddComponent(artifactResourceComponentData);

            _itemRegistry.CreateItem("Tritonite",
                "A rare and precious gemstone found by vanquishing the formidable Krakolith." +
                " This gem holds the power to unlock new realms of knowledge." +
                " Keep it safe and wear it proudly as a badge of honor!",
                ItemType.CreatureDrop, "ItemIcons/test");

            // Smelter Items

            ItemData plate = _itemRegistry.CreateItem("Seashell Plate",
                "Finally, a use for all those seashells you've been collecting on the beach!", ItemType.Metal,
                "ItemIcons/test");
            ItemData glass = _itemRegistry.CreateItem("Sea Glass",
                "The shimmering beauty of this glass is unmatched. It captures the colors of the ocean and adds a touch of elegance to any setting.",
                ItemType.Resource, "ItemIcons/test");
            ItemData silicon = _itemRegistry.CreateItem("Silicon",
                "A vital component in many technological advancements, Silicon is an essential material for any inventor or craftsman.",
                ItemType.Resource, "ItemIcons/test");
            ItemData abyssalCrystal = _itemRegistry.CreateItem("Abyssal Crystal",
                "For when you need a crystal that's as dark and mysterious as your soul. It has magical properties that can enhance a variety of items.",
                ItemType.Resource, "ItemIcons/test");
            ItemData mermaidMetal = _itemRegistry.CreateItem("Mermaid Metal",
                "This sturdy metal is both beautiful and functional. Its unique composition makes it resistant to rust and corrosion, making it ideal for use in marine environments.",
                ItemType.Metal, "ItemIcons/test");
            ItemData coralSteel = _itemRegistry.CreateItem("Coral Steel",
                "A type of steel that has been reinforced with coral fragments, giving it extra strength and durability.",
                ItemType.Metal, "ItemIcons/test");
            ItemData calAbys = _itemRegistry.CreateItem("Calcified Abyssallite",
                "Why settle for regular old metal when you can have something that sounds like a fancy medical condition?",
                ItemType.Metal, "ItemIcons/test");
            ItemData glow = _itemRegistry.CreateItem("Twilight Glow",
                "This mysterious material emits a soft, ethereal glow. Its uses are not yet fully understood, but its potential is limitless.",
                ItemType.Resource, "ItemIcons/test");
            ItemData soil = _itemRegistry.CreateItem("Soil", "A pile of dirt. It's not much, but it's all you've got.",
                ItemType.Resource, "ItemIcons/test");
            ResourceComponentData soilResourceComponentData = new(100);
            soil.AddComponent(soilResourceComponentData);

            // Fabricator Items

            ItemData cloth = _itemRegistry.CreateItem("Cloth",
                "Who knew that a bunch of fibers could be turned into something so soft and comfortable?",
                ItemType.Resource, "ItemIcons/test");
            ItemData plank = _itemRegistry.CreateItem("Wood Plank",
                "A staple of shipbuilding. They're strong and lightweight, making them ideal for a variety of construction projects.",
                ItemType.Resource, "ItemIcons/test", new[,]
                {
                    { true, true }
                });
            ItemData plasma = _itemRegistry.CreateItem("Laser Gun Plasma",
                "This high-tech material is a key component in laser weaponry. Its properties allow for highly concentrated and precise beams of energy.",
                ItemType.Weapon, "ItemIcons/test");
            ItemData board = _itemRegistry.CreateItem("Circuit Board",
                "The puzzle that makes your computer go beep boop.",
                ItemType.Resource, "ItemIcons/test");
            ItemData hook = _itemRegistry.CreateItem("Hook",
                "Great for pulling yourself back from the edge of insanity.",
                ItemType.Resource, "ItemIcons/test");
            ItemData metalRod =
                _itemRegistry.CreateItem("Metal Rod", "A stick, but fancier.", ItemType.Metal, "ItemIcons/test");
            ItemData deepPlate = _itemRegistry.CreateItem("Deep Sea Infused Plate",
                "A thick, sturdy plate made of metal or composite material, used to protect the body from harm in combat or other dangerous situations.",
                ItemType.Metal, "ItemIcons/test");
            ItemData torpedo = _itemRegistry.CreateItem("Torpedo", "For use only with the Torpedo Launcher.",
                ItemType.Weapon,
                "ItemIcons/test");

            // Consumables

            ItemData orange = _itemRegistry.CreateItem("Orange",
                "Prickly fruits for a poke-filled harvest. Watch out for juicy surprises!", ItemType.Fruit,
                "ItemIcons/test");
            
            ConsumableComponentData consumableComponentData = new(5, 10);
            orange.AddComponent(consumableComponentData);
            ResourceComponentData orangeComponentData = new(2);
            orange.AddComponent(orangeComponentData);

            ItemData quarkberry = _itemRegistry.CreateItem("Quarkberries", "Sweet and peculiar, just like this place.",
                ItemType.Fruit,
                "ItemIcons/test");
            consumableComponentData = new ConsumableComponentData(10, 5);
            quarkberry.AddComponent(consumableComponentData);

            ItemData meat = _itemRegistry.CreateItem("Sea Meat", "Savory bounty from the ocean's creatures.",
                ItemType.Consumable,
                "ItemIcons/test");
            consumableComponentData = new ConsumableComponentData(10, 10);
            meat.AddComponent(consumableComponentData);

            ItemData jam = _itemRegistry.CreateItem("Orange Jam",
                "For when you want a meal that's both juicy and slimy. Yum!", ItemType.Consumable, "ItemIcons/test");
            consumableComponentData = new ConsumableComponentData(15, 20);
            jam.AddComponent(consumableComponentData);

            ItemData stew = _itemRegistry.CreateItem("Fish Stew",
                "For when you want a meal that's both prickly and slimy. Yum!", ItemType.Consumable, "ItemIcons/test");
            consumableComponentData = new ConsumableComponentData(20, 30);
            stew.AddComponent(consumableComponentData);

            ItemData seafood = _itemRegistry.CreateItem("Questionable Seafood",
                "This dish may make you question your life choices, but at least you'll have a full belly." +
                " Just make sure you have plenty of water on hand, because it's guaranteed to make you thirsty." +
                " And maybe keep a bucket nearby, just in case.",
                ItemType.Consumable, "ItemIcons/test");
            consumableComponentData = new ConsumableComponentData(25, 50);
            seafood.AddComponent(consumableComponentData);

            ItemData schnitzel = _itemRegistry.CreateItem("Leviathan Schnitzel",
                "It's time for payback! Feast on the crispy Leviathan skin and feel" +
                " the thrill of victory over the once-mighty sea creature.",
                ItemType.Consumable, "ItemIcons/test");
            consumableComponentData = new ConsumableComponentData(30, 80);
            schnitzel.AddComponent(consumableComponentData);

            ItemData crunch = _itemRegistry.CreateItem("Crunchy Delight",
                "It's so delicious, it'll bring tears to your eyes." +
                " Whether it's tears of joy or tears of pain is up to you.",
                ItemType.Consumable, "ItemIcons/test");
            consumableComponentData = new ConsumableComponentData(-10, 80);
            crunch.AddComponent(consumableComponentData);

            ItemData ointment = _itemRegistry.CreateItem("Ointment",
                "This ointment is a must-have for any researcher." +
                " Its healing properties make it ideal for treating cuts, scrapes, and other injuries." +
                " Wait, you're not eating it are you?",
                ItemType.Consumable, "ItemIcons/test");
            consumableComponentData = new ConsumableComponentData(50, 5);
            ointment.AddComponent(consumableComponentData);

            // Machines

            ItemData pot = _itemRegistry.CreateItem("Cooking Pot",
                "A device that can cook a variety of ingredients into a hearty stew," +
                " with the help of a built-in mixer and heating elements.",
                ItemType.Machine, "ItemIcons/test-missing", new[,]
                {
                    { false, true, true, true },
                    { true, true, true, true },
                    { true, false, true, true },
                    { true, true, true, false }
                });
            PlaceableComponentData placeableComponentData = new(
                new SerializableGameObject(
                    "PlaceableObjects/CookingPot",
                    Vector3.zero,
                    Quaternion.identity
                )
            );
            pot.AddComponent(placeableComponentData);

            ItemData fabricator = _itemRegistry.CreateItem("Fabricator",
                "A multi-purpose machine that can be used to craft a wide range of items from various materials." +
                " It uses a combination of robotic arms, lasers, and 3D printing technology.",
                ItemType.Machine, "ItemIcons/test", new[,]
                {
                    { false, false, false, false },
                    { false, true, true, false },
                    { false, true, false, false },
                    { false, false, false, false }
                });
            placeableComponentData = new PlaceableComponentData(
                new SerializableGameObject(
                    "PlaceableObjects/Fabricator",
                    Vector3.zero,
                    Quaternion.identity
                )
            );
            fabricator.AddComponent(placeableComponentData);

            ItemData smelter = _itemRegistry.CreateItem("Smelter",
                "A high-temperature furnace that can melt down minerals and metals into more useful forms." +
                " It's capable of producing alloys and other complex materials.",
                ItemType.Machine, "ItemIcons/test");
            placeableComponentData = new PlaceableComponentData(
                new SerializableGameObject(
                    "PlaceableObjects/Smelter",
                    Vector3.zero,
                    Quaternion.identity
                )
            );
            smelter.AddComponent(placeableComponentData);

            ItemData assembler = _itemRegistry.CreateItem("Assembler",
                "A device that can be used to construct complex objects from smaller parts." +
                " It uses precise robotic arms and advanced algorithms to put everything together.",
                ItemType.Machine, "ItemIcons/test");
            placeableComponentData = new PlaceableComponentData(
                new SerializableGameObject(
                    "PlaceableObjects/Assembler",
                    Vector3.zero,
                    Quaternion.identity
                )
            );
            assembler.AddComponent(placeableComponentData);


            // Tools

            ItemData fishNet = _itemRegistry.CreateItem("Fish Net",
                "Finally, a way to catch fish without all that tedious fishing. Just toss it overboard and let it do the work for you.",
                ItemType.Tool, "ItemIcons/test");
            ToolComponentData toolComponentData = new(0, 30, "Tools/FishNet");
            fishNet.AddComponent(toolComponentData);

            ItemData glowPods = _itemRegistry.CreateItem("Sticky Glow Pods",
                "These pods stick to just about anything and emit a soft, comforting glow. They're perfect for lighting up dark corners of caves.",
                ItemType.Tool, "ItemIcons/test");
            toolComponentData = new ToolComponentData(0, 30, "Tools/GlowPods");
            glowPods.AddComponent(toolComponentData);

            ItemData boat = _itemRegistry.CreateItem("Boat",
                "Because sometimes you just need to sail away from all your problems.",
                ItemType.Tool, "ItemIcons/test");
            placeableComponentData = new PlaceableComponentData(
                new SerializableGameObject(
                    "PlaceableObjects/Boat", Vector3.zero, Quaternion.identity
                )
            );
            boat.AddComponent(placeableComponentData);

            ItemData anchorLine = _itemRegistry.CreateItem("Anchor Line",
                "Allows you to quickly ascend to your boat. Needs a direct line of sight.", ItemType.Tool,
                "ItemIcons/test");
            toolComponentData = new ToolComponentData(0, 30, "Tools/AnchorLine");
            anchorLine.AddComponent(toolComponentData);

            ItemData sonar = _itemRegistry.CreateItem("Sonar",
                "Tired of being ambushed by angry sea creatures? Fear not! This sonar lets you detect enemies before they detect you, and locate valuable resources to boot.",
                ItemType.Tool, "ItemIcons/test");
            toolComponentData = new ToolComponentData(0, 30, "Tools/Sonar");
            sonar.AddComponent(toolComponentData);

            ItemData reaper = _itemRegistry.CreateItem("Reaper’s Call",
                "The dread summons, a beckoning from beyond. Pray to your gods, for when it answers, all shall be consumed by the abyss.",
                ItemType.Tool, "ItemIcons/test");
            toolComponentData = new ToolComponentData(0, 30, "Tools/ReapersCall");
            reaper.AddComponent(toolComponentData);

            // Equipment

            ItemData flippers = _itemRegistry.CreateItem("Flippers",
                "When you want to swim like a fish, but still keep your human dignity.",
                ItemType.Equipment, "ItemIcons/test");
            EquipmentComponentData equipmentComponentData =
                new(EquipmentSlotType.Feet, 30, new List<Tuple<Enhancements, int>>
                {
                    new(Enhancements.SwimmingSpeed, 6),
                    new(Enhancements.Speed, -2),
                });
            flippers.AddComponent(equipmentComponentData);

            ItemData oxygenTank = _itemRegistry.CreateItem("Oxygen Tank",
                "This essential piece of diving equipment lets you stay submerged and explore for extended periods without worrying about running out of breath.",
                ItemType.Equipment, "ItemIcons/test", new bool[,] {{true},{true}});
            equipmentComponentData =
                new EquipmentComponentData(EquipmentSlotType.Body, 30, new List<Tuple<Enhancements, int>>
                {
                    new(Enhancements.OxygenCapacity, 10),
                    new(Enhancements.SwimmingSpeed, -1),
                    new(Enhancements.Speed, -1),
                });
            oxygenTank.AddComponent(equipmentComponentData);

            // ItemData suit = _itemRegistry.CreateItem("Pressure Suit",
            //     "Now you can explore the deep sea without getting crushed by the pressure.", ItemType.Equipment,
            //     "ItemIcons/test");
            // equipmentComponentData =
            //     new EquipmentComponentData(0, 30, new List<Tuple<Enhancements, int>> { new( "pressureCapacity", 10) });
            // suit.AddComponent(equipmentComponentData);

            ItemData abyssalTank = _itemRegistry.CreateItem("Abyssal Tank",
                "This tank will keep you alive in the darkest depths of the ocean, but at what cost? Who knows what horrors lurk down there…",
                ItemType.Equipment, "ItemIcons/test", new bool[,] {{true},{true}});
            equipmentComponentData =
                new EquipmentComponentData(EquipmentSlotType.Body, 30, new List<Tuple<Enhancements, int>>
                {
                    new(Enhancements.OxygenCapacity, 30),
                    new(Enhancements.SwimmingSpeed, -3),
                    new(Enhancements.Speed, -3),
                });
            abyssalTank.AddComponent(equipmentComponentData);

            // ItemData armoredSuit = _itemRegistry.CreateItem("Armored Pressure Suit",
            //     "Upgraded pressure suit for cave diving with enhanced protection and pressure handling.",
            //     ItemType.Equipment, "ItemIcons/test");
            // equipmentComponentData =
            //     new EquipmentComponentData(0, 30,
            //         new List<Tuple<Enhancements, int>> { new("pressureCapacity", 30), new("armor", 20) });
            // armoredSuit.AddComponent(equipmentComponentData);

            ItemData drill = _itemRegistry.CreateItem("Suction Drill",
                "Finally, a drill that sucks in a good way! Perfect for those who want to dig deep and collect precious materials without breaking a sweat.",
                ItemType.Equipment, "ItemIcons/test");
            equipmentComponentData =
                new EquipmentComponentData(EquipmentSlotType.Head, 30, new List<Tuple<Enhancements, int>> { new(Enhancements.DiggingSpeed, 10) });
            drill.AddComponent(equipmentComponentData);

            // Weapons

            ItemData spear = _itemRegistry.CreateItem("Spear",
                "The perfect tool for when you want to stab something, but don't want to get too close.",
                ItemType.Weapon,
                "ItemIcons/test");
            WeaponComponentData weaponComponentData = new(0, 30, "Weapons/Spear");
            spear.AddComponent(weaponComponentData);

            ItemData laser = _itemRegistry.CreateItem("Laser Gun",
                "An advanced and versatile weapon that uses laser technology to deliver devastating energy beams. Handle with caution.",
                ItemType.Weapon, "ItemIcons/test");
            weaponComponentData = new WeaponComponentData(0, 30, "Weapons/Laser");
            laser.AddComponent(weaponComponentData);

            ItemData trident = _itemRegistry.CreateItem("Trident",
                "The perfect weapon for your underwater adventures or your 'Little Mermaid' cosplay, whichever comes first.",
                ItemType.Weapon, "ItemIcons/test");
            weaponComponentData = new WeaponComponentData(0, 30, "Weapons/Trident");
            trident.AddComponent(weaponComponentData);

            ItemData torpedoLauncher = _itemRegistry.CreateItem("Torpedo Launcher",
                "Want to send a message to that pesky Leviathan that's been harassing you? Look no further than the Torpedo Launcher!",
                ItemType.Weapon, "ItemIcons/test");
            weaponComponentData = new WeaponComponentData(0, 30, "Weapons/TorpedoLauncher");
            torpedoLauncher.AddComponent(weaponComponentData);

            // Write to JSON

            ItemData[] items = _itemRegistry.GetAll();
            string json = DataSerializer.SerializeItemData(items);
            File.WriteAllText("Assets/Resources/ItemData.json", json);

            string nameToHash = items.Aggregate("", (current, item) => current + item.Name + ": " + item.ID + "\n");
            File.WriteAllText("Assets/Resources/ItemHashes.txt", nameToHash);

            // Create recipes

            // Consumables

            _recipeRegistry.CreateCraftingRecipe(jam.ID,
                MachineType.CookingPot,
                new Dictionary<string, int>
                {
                    { orange.ID, 1 },
                    { seaweed.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(stew.ID,
                MachineType.CookingPot,
                new Dictionary<string, int>
                {
                    { meat.ID, 1 },
                    { seaweed.ID, 1 },
                    { quarkberry.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(seafood.ID,
                MachineType.CookingPot,
                new Dictionary<string, int>
                {
                    { meat.ID, 1 },
                    { tentacle.ID, 1 },
                    { seaweed.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(schnitzel.ID,
                MachineType.CookingPot,
                new Dictionary<string, int>
                {
                    { meat.ID, 1 },
                    { skin.ID, 1 },
                    { quarkberry.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(crunch.ID,
                MachineType.CookingPot,
                new Dictionary<string, int>
                {
                    { tear.ID, 1 },
                    { seaweed.ID, 2 },
                    { fiber.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(ointment.ID,
                MachineType.Fabricator | MachineType.PocketFabricator,
                new Dictionary<string, int>
                {
                    { spines.ID, 1 },
                    { fiber.ID, 1 }
                });

            // Resources

            _recipeRegistry.CreateCraftingRecipe(cloth.ID,
                MachineType.Fabricator | MachineType.PocketFabricator,
                new Dictionary<string, int>
                {
                    { fiber.ID, 2 }
                });

            _recipeRegistry.CreateCraftingRecipe(plank.ID,
                MachineType.Fabricator | MachineType.PocketFabricator,
                new Dictionary<string, int>
                {
                    { wood.ID, 1 },
                    { fiber.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(glass.ID,
                MachineType.Smelter,
                new Dictionary<string, int>
                {
                    { coralium.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(silicon.ID,
                MachineType.Smelter,
                new Dictionary<string, int>
                {
                    { soil.ID, 1 }
                });


            _recipeRegistry.CreateCraftingRecipe(board.ID,
                MachineType.Fabricator,
                new Dictionary<string, int>
                {
                    { plate.ID, 1 },
                    { silicon.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(abyssalCrystal.ID,
                MachineType.Smelter,
                new Dictionary<string, int>
                {
                    { abyssallite.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(hook.ID,
                MachineType.Fabricator,
                new Dictionary<string, int>
                {
                    { mermaidMetal.ID, 2 }
                });

            _recipeRegistry.CreateCraftingRecipe(glow.ID,
                MachineType.Smelter,
                new Dictionary<string, int>
                {
                    { lumini.ID, 1 },
                    { doomstone.ID, 1 }
                });


            // Metal

            _recipeRegistry.CreateCraftingRecipe(plate.ID,
                MachineType.Smelter,
                new Dictionary<string, int>
                {
                    { calcium.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(mermaidMetal.ID,
                MachineType.Smelter,
                new Dictionary<string, int>
                {
                    { kelp.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(coralSteel.ID,
                MachineType.Smelter,
                new Dictionary<string, int>
                {
                    { coralium.ID, 1 },
                    { kelp.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(calAbys.ID,
                MachineType.Smelter,
                new Dictionary<string, int>
                {
                    { calcium.ID, 1 },
                    { abyssallite.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(deepPlate.ID,
                MachineType.Fabricator,
                new Dictionary<string, int>
                {
                    { plate.ID, 1 },
                    { coralSteel.ID, 1 },
                    { abyssalCrystal.ID, 1 }
                });


            // Machines

            _recipeRegistry.CreateCraftingRecipe(fabricator.ID,
                MachineType.Fabricator | MachineType.PocketFabricator,
                new Dictionary<string, int>
                {
                    { calcium.ID, 1 },
                    { coralium.ID, 2 }
                });

            _recipeRegistry.CreateCraftingRecipe(pot.ID,
                MachineType.Fabricator,
                new Dictionary<string, int>
                {
                    { coralium.ID, 2 },
                    { scales.ID, 1 }
                });


            _recipeRegistry.CreateCraftingRecipe(smelter.ID,
                MachineType.Fabricator,
                new Dictionary<string, int>
                {
                    { coralium.ID, 2 },
                    { calcium.ID, 1 },
                    { scales.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(assembler.ID,
                MachineType.Fabricator,
                new Dictionary<string, int>
                {
                    { board.ID, 2 },
                    { glass.ID, 1 },
                    { fabricator.ID, 1 }
                });


            // Tools

            _recipeRegistry.CreateCraftingRecipe(fishNet.ID,
                MachineType.Fabricator | MachineType.PocketFabricator,
                new Dictionary<string, int>
                {
                    { fiber.ID, 2 },
                    { seaweed.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(boat.ID,
                MachineType.Fabricator,
                new Dictionary<string, int>
                {
                    { plank.ID, 2 },
                    { cloth.ID, 1 },
                    { plate.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(anchorLine.ID,
                MachineType.Assembler,
                new Dictionary<string, int>
                {
                    { tentacle.ID, 3 },
                    { hook.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(glowPods.ID,
                MachineType.Fabricator | MachineType.PocketFabricator,
                new Dictionary<string, int>
                {
                    { lumini.ID, 1 },
                    { tentacle.ID, 1 }
                },
                16);

            _recipeRegistry.CreateCraftingRecipe(sonar.ID,
                MachineType.Assembler,
                new Dictionary<string, int>
                {
                    { glow.ID, 1 },
                    { abyssalCrystal.ID, 1 },
                    { board.ID, 2 }
                });

            _recipeRegistry.CreateCraftingRecipe(reaper.ID,
                MachineType.Assembler,
                new Dictionary<string, int>
                {
                    { artifact.ID, 2 },
                    { eye.ID, 1 },
                    { tentacle.ID, 1 },
                    { tooth.ID, 1 },
                    { glow.ID, 1 }
                });


            // Weapons  

            _recipeRegistry.CreateCraftingRecipe(spear.ID,
                MachineType.Fabricator | MachineType.PocketFabricator,
                new Dictionary<string, int>
                {
                    { plank.ID, 1 },
                    { coralium.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(trident.ID,
                MachineType.Assembler,
                new Dictionary<string, int>
                {
                    { metalRod.ID, 1 },
                    { abyssalCrystal.ID, 1 },
                    { tear.ID, 1 },
                    { tooth.ID, 3 }
                });

            _recipeRegistry.CreateCraftingRecipe(plasma.ID,
                MachineType.Fabricator | MachineType.PocketFabricator,
                new Dictionary<string, int>
                {
                    { silicon.ID, 1 }
                });

            _recipeRegistry.CreateCraftingRecipe(laser.ID,
                MachineType.Assembler,
                new Dictionary<string, int>
                {
                    { calAbys.ID, 2 },
                    { coralSteel.ID, 1 },
                    { board.ID, 1 },
                    { tear.ID, 1 }
                });


            _recipeRegistry.CreateCraftingRecipe(torpedo.ID,
                MachineType.Fabricator,
                new Dictionary<string, int>
                {
                    { spines.ID, 1 },
                    { coralSteel.ID, 1 }
                },
                4);

            _recipeRegistry.CreateCraftingRecipe(torpedoLauncher.ID,
                MachineType.Assembler,
                new Dictionary<string, int>
                {
                    { glow.ID, 2 },
                    { deepPlate.ID, 2 },
                    { board.ID, 1 },
                    { mermaidMetal.ID, 1 }
                });


            // Equipment


            _recipeRegistry.CreateCraftingRecipe(flippers.ID,
                MachineType.Fabricator,
                new Dictionary<string, int>
                {
                    { cloth.ID, 2 },
                    { scales.ID, 2 }
                });


            _recipeRegistry.CreateCraftingRecipe(oxygenTank.ID,
                MachineType.Fabricator,
                new Dictionary<string, int>
                {
                    { glass.ID, 2 },
                    { plate.ID, 1 },
                    { silicon.ID, 1 }
                });


            // _recipeRegistry.CreateCraftingRecipe(suit.ID,
            //     MachineType.Assembler,
            //     new Dictionary<string, int>
            //     {
            //         { cloth.ID, 2 },
            //         { plate.ID, 1 },
            //         { glass.ID, 1 },
            //         { silicon.ID, 1 }
            //     });

            _recipeRegistry.CreateCraftingRecipe(abyssalTank.ID,
                MachineType.Assembler,
                new Dictionary<string, int>
                {
                    { mermaidMetal.ID, 2 },
                    { abyssalCrystal.ID, 2 },
                    { board.ID, 1 }
                });

            // _recipeRegistry.CreateCraftingRecipe(armoredSuit.ID,
            //     MachineType.Assembler,
            //     new Dictionary<string, int>
            //     {
            //         { deepPlate.ID, 2 },
            //         { suit.ID, 1 },
            //         { skin.ID, 1 }
            //     });


            _recipeRegistry.CreateCraftingRecipe(drill.ID,
                MachineType.Assembler,
                new Dictionary<string, int>
                {
                    { doomstone.ID, 1 },
                    { abyssalCrystal.ID, 1 },
                    { coralSteel.ID, 1 }
                });


            IEnumerable<CraftingRecipe> recipes = _recipeRegistry.GetAll();
            json = DataSerializer.SerializeRecipeData(recipes);
            File.WriteAllText("Assets/Resources/RecipeData.json", json);

            _itemRegistry.SetInitialized();
            _recipeRegistry.SetInitialized();
        }
    }
}