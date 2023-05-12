using System;
using System.Collections.Generic;
using System.IO;
using Items;
using Newtonsoft.Json;
using UnityEngine;

namespace DataManager
{
    /// <summary>
    ///     Used only once to generate the ItemData.json file.
    ///     This file is then used to load items into the game.
    ///     Hence, this script is not needed in the final build.
    /// </summary>
    public class DataGenerator : MonoBehaviour
    {
        public ItemRegistry itemRegistry;

        private void CreateItems()
        {
             // Simple resources from gathering and monsters, without any components.
            itemRegistry.CreateItem("Fiber", "Tangled treasures from island grass.", ItemType.Resource,
                "ItemIcons/test");
            itemRegistry.CreateItem("Wood",
                "Sun-kissed timber harvested from majestic palm trees." +
                " Ideal for crafting and roasting marshmallows (if you can find any).",
                ItemType.Resource, "ItemIcons/test");
            itemRegistry.CreateItem("Seaweed Fiber", "Seashore secret for crafting needs.", ItemType.Resource,
                "ItemIcons/test");
            itemRegistry.CreateItem("Seashell Calcium", "Calcium-rich treasures from seashells.", ItemType.Resource,
                "ItemIcons/test");
            itemRegistry.CreateItem("Fish Scales", "Gleaming scales from the planet’s fishes.", ItemType.CreatureDrop,
                "ItemIcons/test");
            itemRegistry.CreateItem("Coralium", "Vibrant oceanic resource harvested from corals.", ItemType.Resource,
                "ItemIcons/test");
            itemRegistry.CreateItem("Abyssallite",
                "Deep-sea gem, hidden in the depths. A rare find for adventurous divers.", ItemType.Resource,
                "ItemIcons/test");
            itemRegistry.CreateItem("Kelp Iron", "Iron with a touch of kelp magic.", ItemType.Resource,
                "ItemIcons/test");
            itemRegistry.CreateItem("Sea Urchin Spines", "Prickly prizes from deep-sea urchins.", ItemType.CreatureDrop,
                "ItemIcons/test");
            itemRegistry.CreateItem("Leviathan Skin", "A testament to conquering the sea's mightiest.",
                ItemType.CreatureDrop, "ItemIcons/test");
            itemRegistry.CreateItem("Leviathan Tooth",
                "Enormous tooth of the legendary Leviathan. Handle carefully, display proudly.", ItemType.CreatureDrop,
                "ItemIcons/test");
            itemRegistry.CreateItem("Crystalline Tear", "Glistening tear shed by the Blackened Eye.",
                ItemType.CreatureDrop, "ItemIcons/test");
            itemRegistry.CreateItem("Deep Tentacle", "Sinister tentacle harvested from the dreaded Devourer.",
                ItemType.CreatureDrop, "ItemIcons/test");
            itemRegistry.CreateItem("Luminitite", "Glowing mineral illuminating the caves.", ItemType.Resource,
                "ItemIcons/test");
            itemRegistry.CreateItem("Doomstone", "Foreboding stone found in the depths of the island's caves.",
                ItemType.Resource, "ItemIcons/test");
            itemRegistry.CreateItem("Dreadworm Eye",
                "Eerie eye, snatched from the fearsome Dreadworm. Peer into its unsettling gaze.",
                ItemType.CreatureDrop, "ItemIcons/test");
            itemRegistry.CreateItem("Alien Artifact",
                "From the depths of the cave comes this mysterious artifact." +
                " Is it a remnant of an ancient civilization or something else?",
                ItemType.Resource, "ItemIcons/test");
            itemRegistry.CreateItem("Tritonite",
                "A rare and precious gemstone found by vanquishing the formidable Krakolith." +
                " This gem holds the power to unlock new realms of knowledge." +
                " Keep it safe and wear it proudly as a badge of honor!",
                ItemType.CreatureDrop, "ItemIcons/test");

            // Smelter Items

            itemRegistry.CreateItem("Seashell Plate",
                "Finally, a use for all those seashells you've been collecting on the beach!", ItemType.Metal,
                "ItemIcons/test");
            itemRegistry.CreateItem("Sea Glass",
                "The shimmering beauty of this glass is unmatched. It captures the colors of the ocean and adds a touch of elegance to any setting.",
                ItemType.Resource, "ItemIcons/test");
            itemRegistry.CreateItem("Silicon",
                "A vital component in many technological advancements, Silicon is an essential material for any inventor or craftsman.",
                ItemType.Resource, "ItemIcons/test");
            itemRegistry.CreateItem("Abyssal Crystal",
                "For when you need a crystal that's as dark and mysterious as your soul. It has magical properties that can enhance a variety of items.",
                ItemType.Resource, "ItemIcons/test");
            itemRegistry.CreateItem("Mermaid Metal",
                "This sturdy metal is both beautiful and functional. Its unique composition makes it resistant to rust and corrosion, making it ideal for use in marine environments.",
                ItemType.Metal, "ItemIcons/test");
            itemRegistry.CreateItem("Coral Steel",
                "A type of steel that has been reinforced with coral fragments, giving it extra strength and durability.",
                ItemType.Metal, "ItemIcons/test");
            itemRegistry.CreateItem("Calcified Abyssallite",
                "Why settle for regular old metal when you can have something that sounds like a fancy medical condition?",
                ItemType.Metal, "ItemIcons/test");
            itemRegistry.CreateItem("Twilight Glow",
                "This mysterious material emits a soft, ethereal glow. Its uses are not yet fully understood, but its potential is limitless.",
                ItemType.Resource, "ItemIcons/test");

            // Fabricator Items

            itemRegistry.CreateItem("Cloth",
                "Who knew that a bunch of fibers could be turned into something so soft and comfortable?",
                ItemType.Resource, "ItemIcons/test");
            itemRegistry.CreateItem("Wood Plank",
                "A staple of shipbuilding. They're strong and lightweight, making them ideal for a variety of construction projects.",
                ItemType.Resource, "ItemIcons/test");
            itemRegistry.CreateItem("Laser Gun Plasma",
                "This high-tech material is a key component in laser weaponry. Its properties allow for highly concentrated and precise beams of energy.",
                ItemType.Weapon, "ItemIcons/test");
            itemRegistry.CreateItem("Circuit Board", "The puzzle that makes your computer go beep boop.",
                ItemType.Resource, "ItemIcons/test");
            itemRegistry.CreateItem("Hook", "Great for pulling yourself back from the edge of insanity.",
                ItemType.Resource, "ItemIcons/test");
            itemRegistry.CreateItem("Metal Rod", "A stick, but fancier.", ItemType.Metal, "ItemIcons/test");
            itemRegistry.CreateItem("Deep Sea Infused Plate",
                "A thick, sturdy plate made of metal or composite material, used to protect the body from harm in combat or other dangerous situations.",
                ItemType.Metal, "ItemIcons/test");
            itemRegistry.CreateItem("Torpedo", "For use only with the Torpedo Launcher.", ItemType.Weapon,
                "ItemIcons/test");

            // Consumables

            Item thornmelon = itemRegistry.CreateItem("Thornmelons",
                "Prickly fruits for a poke-filled harvest. Watch out for thorny surprises!", ItemType.Fruit,
                "ItemIcons/test");
            ConsumableComponent consumableComponent = new(5, 10);
            thornmelon.AddComponent(consumableComponent);

            Item quarkberry = itemRegistry.CreateItem("Quarkberries", "Sweet and peculiar, just like this place.",
                ItemType.Fruit,
                "ItemIcons/test");
            consumableComponent = new ConsumableComponent(10, 5);
            quarkberry.AddComponent(consumableComponent);

            Item meat = itemRegistry.CreateItem("Sea Meat", "Savory bounty from the ocean's creatures.",
                ItemType.Consumable,
                "ItemIcons/test");
            consumableComponent = new ConsumableComponent(10, 10);
            meat.AddComponent(consumableComponent);

            Item jam = itemRegistry.CreateItem("Thornmelon Jam",
                "For when you want a meal that's both prickly and slimy. Yum!", ItemType.Consumable, "ItemIcons/test");
            consumableComponent = new ConsumableComponent(15, 20);
            jam.AddComponent(consumableComponent);

            Item stew = itemRegistry.CreateItem("Fish Stew",
                "For when you want a meal that's both prickly and slimy. Yum!", ItemType.Consumable, "ItemIcons/test");
            consumableComponent = new ConsumableComponent(20, 30);
            stew.AddComponent(consumableComponent);

            Item seafood = itemRegistry.CreateItem("Questionable Seafood",
                "This dish may make you question your life choices, but at least you'll have a full belly." +
                " Just make sure you have plenty of water on hand, because it's guaranteed to make you thirsty." +
                " And maybe keep a bucket nearby, just in case.",
                ItemType.Consumable, "ItemIcons/test");
            consumableComponent = new ConsumableComponent(25, 50);
            seafood.AddComponent(consumableComponent);

            Item schnitzel = itemRegistry.CreateItem("Leviathan Schnitzel",
                "It's time for payback! Feast on the crispy Leviathan skin and feel" +
                " the thrill of victory over the once-mighty sea creature.",
                ItemType.Consumable, "ItemIcons/test");
            consumableComponent = new ConsumableComponent(30, 80);
            schnitzel.AddComponent(consumableComponent);

            Item crunch = itemRegistry.CreateItem("Crunchy Delight",
                "It's so delicious, it'll bring tears to your eyes." +
                " Whether it's tears of joy or tears of pain is up to you.",
                ItemType.Consumable, "ItemIcons/test");
            consumableComponent = new ConsumableComponent(-10, 80);
            crunch.AddComponent(consumableComponent);

            Item ointment = itemRegistry.CreateItem("Ointment",
                "This ointment is a must-have for any researcher." +
                " Its healing properties make it ideal for treating cuts, scrapes, and other injuries." +
                " Wait, you're not eating it are you?",
                ItemType.Consumable, "ItemIcons/test");
            consumableComponent = new ConsumableComponent(50, 5);
            ointment.AddComponent(consumableComponent);

            // Machines

            Item pot = itemRegistry.CreateItem("Cooking Pot",
                "A device that can cook a variety of ingredients into a hearty stew," +
                " with the help of a built-in mixer and heating elements.",
                ItemType.Machine, "ItemIcons/test");
            PlaceableComponent placeableComponent = new(
                new SerializableGameObject(
                    "PlaceableObjects/CookingPot",
                    Vector3.zero,
                    Quaternion.identity
                )
            );
            pot.AddComponent(placeableComponent);

            Item fabricator = itemRegistry.CreateItem("Fabricator",
                "A multi-purpose machine that can be used to craft a wide range of items from various materials." +
                " It uses a combination of robotic arms, lasers, and 3D printing technology.",
                ItemType.Machine, "ItemIcons/test");
            placeableComponent = new PlaceableComponent(
                new SerializableGameObject(
                    "PlaceableObjects/Fabricator",
                    Vector3.zero,
                    Quaternion.identity
                )
            );
            fabricator.AddComponent(placeableComponent);

            Item smelter = itemRegistry.CreateItem("Smelter",
                "A high-temperature furnace that can melt down minerals and metals into more useful forms." +
                " It's capable of producing alloys and other complex materials.",
                ItemType.Machine, "ItemIcons/test");
            placeableComponent = new PlaceableComponent(
                new SerializableGameObject(
                    "PlaceableObjects/Smelter",
                    Vector3.zero,
                    Quaternion.identity
                )
            );
            smelter.AddComponent(placeableComponent);

            Item assembler = itemRegistry.CreateItem("Assembler",
                "A device that can be used to construct complex objects from smaller parts." +
                " It uses precise robotic arms and advanced algorithms to put everything together.",
                ItemType.Machine, "ItemIcons/test");
            placeableComponent = new PlaceableComponent(
                new SerializableGameObject(
                    "PlaceableObjects/Assembler",
                    Vector3.zero,
                    Quaternion.identity
                )
            );
            assembler.AddComponent(placeableComponent);


            // Tools

            Item fishNet = itemRegistry.CreateItem("Fish Net",
                "Finally, a way to catch fish without all that tedious fishing. Just toss it overboard and let it do the work for you.",
                ItemType.Tool, "ItemIcons/test");
            ToolComponent toolComponent = new(0, 30, "Tools/FishNet");
            fishNet.AddComponent(toolComponent);

            Item glowPods = itemRegistry.CreateItem("Sticky Glow Pods",
                "These pods stick to just about anything and emit a soft, comforting glow. They're perfect for lighting up dark corners of caves.",
                ItemType.Tool, "ItemIcons/test");
            toolComponent = new ToolComponent(0, 30, "Tools/GlowPods");
            glowPods.AddComponent(toolComponent);

            Item boat = itemRegistry.CreateItem("Boat",
                "Because sometimes you just need to sail away from all your problems.",
                ItemType.Tool, "ItemIcons/test");
            placeableComponent = new PlaceableComponent(
                new SerializableGameObject(
                    "PlaceableObjects/Boat", Vector3.zero, Quaternion.identity
                )
            );
            boat.AddComponent(placeableComponent);

            Item anchorLine = itemRegistry.CreateItem("Anchor Line",
                "Allows you to quickly ascend to your boat. Needs a direct line of sight.", ItemType.Tool,
                "ItemIcons/test");
            toolComponent = new ToolComponent(0, 30, "Tools/AnchorLine");
            anchorLine.AddComponent(toolComponent);

            Item sonar = itemRegistry.CreateItem("Sonar",
                "Tired of being ambushed by angry sea creatures? Fear not! This sonar lets you detect enemies before they detect you, and locate valuable resources to boot.",
                ItemType.Tool, "ItemIcons/test");
            toolComponent = new ToolComponent(0, 30, "Tools/Sonar");
            sonar.AddComponent(toolComponent);

            Item reaper = itemRegistry.CreateItem("Reaper’s Call",
                "The dread summons, a beckoning from beyond. Pray to your gods, for when it answers, all shall be consumed by the abyss.",
                ItemType.Tool, "ItemIcons/test");
            toolComponent = new ToolComponent(0, 30, "Tools/ReapersCall");
            reaper.AddComponent(toolComponent);

            // Equipment

            Item flippers = itemRegistry.CreateItem("Flippers",
                "When you want to swim like a fish, but still keep your human dignity.",
                ItemType.Equipment, "ItemIcons/test");
            EquipmentComponent equipmentComponent = new(0, 30, new List<Tuple<string, int>> { new("speed", 5) });
            flippers.AddComponent(equipmentComponent);

            Item oxygenTank = itemRegistry.CreateItem("Oxygen Tank",
                "This essential piece of diving equipment lets you stay submerged and explore for extended periods without worrying about running out of breath.",
                ItemType.Equipment, "ItemIcons/test");
            equipmentComponent =
                new EquipmentComponent(0, 30, new List<Tuple<string, int>> { new("oxygenCapacity", 10) });
            oxygenTank.AddComponent(equipmentComponent);

            Item suit = itemRegistry.CreateItem("Pressure Suit",
                "Now you can explore the deep sea without getting crushed by the pressure.", ItemType.Equipment,
                "ItemIcons/test");
            equipmentComponent =
                new EquipmentComponent(0, 30, new List<Tuple<string, int>> { new("pressureCapacity", 10) });
            suit.AddComponent(equipmentComponent);

            Item abyssalTank = itemRegistry.CreateItem("Abyssal Tank",
                "This tank will keep you alive in the darkest depths of the ocean, but at what cost? Who knows what horrors lurk down there…",
                ItemType.Equipment, "ItemIcons/test");
            equipmentComponent =
                new EquipmentComponent(0, 30, new List<Tuple<string, int>> { new("oxygenCapacity", 30) });
            abyssalTank.AddComponent(equipmentComponent);

            Item armoredSuit = itemRegistry.CreateItem("Armored Pressure Suit",
                "Upgraded pressure suit for cave diving with enhanced protection and pressure handling.",
                ItemType.Equipment, "ItemIcons/test");
            equipmentComponent =
                new EquipmentComponent(0, 30,
                    new List<Tuple<string, int>> { new("pressureCapacity", 30), new("armor", 20) });
            armoredSuit.AddComponent(equipmentComponent);

            Item drill = itemRegistry.CreateItem("Suction Drill",
                "Finally, a drill that sucks in a good way! Perfect for those who want to dig deep and collect precious materials without breaking a sweat.",
                ItemType.Equipment, "ItemIcons/test");
            equipmentComponent =
                new EquipmentComponent(0, 30, new List<Tuple<string, int>> { new("digSpeed", 10) });
            drill.AddComponent(equipmentComponent);

            // Weapons

            Item spear = itemRegistry.CreateItem("Spear",
                "The perfect tool for when you want to stab something, but don't want to get too close.",
                ItemType.Weapon,
                "ItemIcons/test");
            WeaponComponent weaponComponent = new(0, 30, "Weapons/Spear");
            spear.AddComponent(weaponComponent);

            Item laser = itemRegistry.CreateItem("Laser Gun",
                "An advanced and versatile weapon that uses laser technology to deliver devastating energy beams. Handle with caution.",
                ItemType.Weapon, "ItemIcons/test");
            weaponComponent = new WeaponComponent(0, 30, "Weapons/Laser");
            laser.AddComponent(weaponComponent);

            Item trident = itemRegistry.CreateItem("Trident",
                "The perfect weapon for your underwater adventures or your 'Little Mermaid' cosplay, whichever comes first.",
                ItemType.Weapon, "ItemIcons/test");
            weaponComponent = new WeaponComponent(0, 30, "Weapons/Trident");
            trident.AddComponent(weaponComponent);

            Item torpedoLauncher = itemRegistry.CreateItem("Torpedo Launcher",
                "Want to send a message to that pesky Leviathan that's been harassing you? Look no further than the Torpedo Launcher!",
                ItemType.Weapon, "ItemIcons/test");
            weaponComponent = new WeaponComponent(0, 30, "Weapons/TorpedoLauncher");
            torpedoLauncher.AddComponent(weaponComponent);

            // Write to JSON

            Item[] items = itemRegistry.GetAll();
            string json = JsonConvert.SerializeObject(items,
                new JsonSerializerSettings
                    { TypeNameHandling = TypeNameHandling.Auto, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            File.WriteAllText("ItemData.json", json);
            
        }
        
        private void Start()
        {
            CreateItems();
            Debug.Log("Done!");
        }
    }
}