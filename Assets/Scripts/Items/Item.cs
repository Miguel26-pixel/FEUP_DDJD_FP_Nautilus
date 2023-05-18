using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;

namespace Items
{
    public static class ItemConstants
    {
        public const int ItemWidth = 4;
        public const int ItemHeight = 4;
    }

    [Serializable]
    public enum ItemType
    {
        Resource,
        Fruit,
        CreatureDrop,
        Consumable,
        Weapon,
        Equipment,
        Machine,
        Metal,
        Tool
    }

    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class ItemData : ItemEntity<ItemComponentData>, IInstantiable<Item>
    {
        [SerializeField] [JsonProperty] private string id;

        [SerializeField] [JsonProperty] private string name;

        [SerializeField] [JsonProperty] private string description;

        [SerializeField] [JsonProperty] protected string iconPath;

        // Only used for categorization of items in the inventory.
        [SerializeField] [JsonProperty] private ItemType type;

        /// <summary>
        ///     Creates a new item with the given id, name, description, and icon.
        /// </summary>
        [JsonConstructor]
        public ItemData(string id, string name, string description, ItemType type, string iconPath,
            bool[,] grid = null)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.type = type;
            this.iconPath = iconPath;

            Grid = grid == null ? ItemGrid.SingleCellGrid() : ItemGrid.ValidateGrid(grid);

            Icons = new Sprite[ItemConstants.ItemHeight, ItemConstants.ItemWidth];
            Sprite[] spriteSheet = Resources.LoadAll<Sprite>(iconPath);

            foreach (Sprite sprite in spriteSheet)
            {
                string spriteName = sprite.name;
                string[] split = spriteName.Split('_');
                int y = int.Parse(split[1]);
                int x = int.Parse(split[2]);

                if (Grid[y, x])
                {
                    Icons[y, x] = sprite;
                }
            }

            Icon = GenerateItemSprite();
        }

        public string ID => id;
        public int IDHash => int.Parse(id, NumberStyles.HexNumber);
        public string Name => name;
        public string Description => description;
        public ItemType Type => type;
        public Sprite[,] Icons { get; }
        public Sprite Icon { get; }
        public BoundsInt Bounds { get; private set; }
        [JsonProperty("grid")] public bool[,] Grid { get; }

        public Item CreateInstance()
        {
            return new Item(this);
        }

        private Sprite GenerateItemSprite()
        {
            int minX = ItemConstants.ItemWidth;
            int minY = ItemConstants.ItemHeight;
            int maxX = -1;
            int maxY = -1;

            int spriteWidth = -1;
            int spriteHeight = -1;

            // Find the bounding box dimensions
            for (int y = 0; y < ItemConstants.ItemHeight; y++)
            {
                for (int x = 0; x < ItemConstants.ItemWidth; x++)
                {
                    if (Grid[y, x])
                    {
                        minX = Math.Min(minX, x);
                        minY = Math.Min(minY, y);
                        maxX = Math.Max(maxX, x);
                        maxY = Math.Max(maxY, y);

                        Sprite sprite = Icons[y, x];
                        if(sprite == null) continue;
                        Rect rect = sprite.rect;

                        spriteWidth = (int)rect.width;
                        spriteHeight = (int)rect.height;
                    }
                }
            }
            
            BoundsInt bounds = new(minX, minY, 0, maxX - minX + 1, maxY - minY + 1, 1);
            Bounds = bounds;

            if (spriteWidth == -1 || spriteHeight == -1)
            {
                return null;
            }

            // Calculate the width and height of the bounding box
            int width = maxX - minX + 1;
            int height = maxY - minY + 1;


            // Create a new texture for the item
            Texture2D texture = new(width * spriteWidth, height * spriteHeight);

            // Set the texture to be transparent
            Color[] transparentPixels = new Color[width * spriteWidth * height * spriteHeight];
            for (int i = 0; i < transparentPixels.Length; i++)
            {
                transparentPixels[i] = Color.clear;
            }

            texture.SetPixels(transparentPixels);

            // Copy the pixels from the sprite sheet to the new texture
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (Grid[y, x])
                    {
                        Sprite sprite = Icons[y, x];
                        if(sprite == null) continue;
                        Rect rect = sprite.rect;

                        Color[] pixels = sprite.texture.GetPixels((int)rect.x,
                            (int)rect.y, (int)rect.width,
                            (int)rect.height);

                        int invertedY = maxY - y;
                        texture.SetPixels((x - minX) * spriteWidth,
                            invertedY * spriteHeight, (int)rect.width,
                            (int)rect.height, pixels);
                    }
                }
            }

            // Apply the changes to the texture
            texture.Apply();

            // Create a new sprite from the texture
            Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, width * spriteWidth, height * spriteHeight),
                new Vector2(0.5f, 0.5f), spriteWidth);

            return newSprite;
        }
    }

    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class Item : ItemEntity<ItemComponent>
    {
        [JsonIgnore] private readonly ItemData _itemData;
        [JsonProperty("id")] private readonly string _itemID;

        public Item(ItemData itemData)
        {
            _itemData = itemData;
            _itemID = itemData.ID;

            foreach (ItemComponentData component in itemData.GetComponents())
            {
                AddComponent(component.CreateInstance());
            }
        }

        public Item(ItemData itemData, List<ItemComponent> components)
        {
            _itemData = itemData;
            _itemID = itemData.ID;
            this.components = components;
        }

        public string ID => _itemID;
        public int IDHash => _itemData.IDHash;
        public string Name => _itemData.Name;
        public string Description => _itemData.Description;
        public ItemType Type => _itemData.Type;
        public Sprite[,] Icons => _itemData.Icons;
        public Sprite Icon => _itemData.Icon;
        public bool[,] Grid => _itemData.Grid;
        public BoundsInt Bounds => _itemData.Bounds;

        public List<ContextMenuAction> GetContextMenuActions()
        {
            List<ContextMenuAction> actions = new();
            foreach (ItemComponent component in components)
            {
                actions.AddRange(component.GetActions());
            }

            return actions;
        }
    }
}