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

        private void Start()
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

            // Weapons

            Item spear = itemRegistry.CreateItem("Spear",
                "The perfect tool for when you want to stab something, but don't want to get too close.",
                ItemType.Weapon,
                "ItemIcons/test");

            // TODO: FILL WITH CORRECT STATS
            WeaponComponent weaponComponent = new(0, 30, 10, 5, 1);
            spear.AddComponent(weaponComponent);

            Item[] items = itemRegistry.GetAll();
            string json = JsonConvert.SerializeObject(items,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            File.WriteAllText("ItemData.json", json);
        }
    }
}