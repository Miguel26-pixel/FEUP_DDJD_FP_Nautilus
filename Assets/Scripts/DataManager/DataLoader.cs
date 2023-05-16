using System;
using System.Collections.Generic;
using Crafting;
using Items;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace DataManager
{
    public class ItemConverter : JsonConverter<Item>
    {
        private readonly ItemRegistry _itemRegistry;

        public ItemConverter(ItemRegistry itemRegistry)
        {
            _itemRegistry = itemRegistry;
        }
        
        // This is probably the worst thing I've ever done in my life
        private readonly Dictionary<Type, Type> _componentInstanceToData = new Dictionary<Type, Type>()
        {
            { typeof(ConsumableComponent), typeof(ConsumableComponentData) },
            { typeof(EquipableComponent), typeof(EquipableComponentData) },
            { typeof(EquipmentComponent), typeof(EquipmentComponentData) },
            { typeof(WeaponComponent), typeof(WeaponComponentData) },
            { typeof(ToolComponent), typeof(ToolComponentData) },
            { typeof(PlaceableComponent), typeof(PlaceableComponentData) },
        };

        public override bool CanWrite => false;
        public override bool CanRead => true;
        
        public override Item ReadJson(JsonReader reader, Type objectType, Item existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            
            string id = (jsonObject["id"] ?? throw new InvalidOperationException()).Value<string>();
            ItemData itemData = _itemRegistry.Get(id);
            if (itemData == null) throw new InvalidOperationException($"Item with id {id} does not exist.");

            JArray componentsArray = (JArray) jsonObject["components"];
            if (componentsArray == null) throw new InvalidOperationException("Item components is not set.");
            List<ItemComponent> components = new();

            foreach (var jToken in componentsArray)
            {
                var componentObject = (JObject)jToken;
                string componentTypeString = (componentObject["$type"] ?? throw new InvalidOperationException()).Value<string>();
                
                // Reflection sins against humanity
                Type componentType = Type.GetType(componentTypeString);
                if (componentType == null) throw new InvalidOperationException($"Component type {componentTypeString} does not exist.");
                
                var getComponentMethod = typeof(ItemData).GetMethod("GetComponent");
                if (getComponentMethod == null) throw new InvalidOperationException("ItemData does not have GetComponent method.");
                
                var componentDataType = _componentInstanceToData[componentType];
                
                var genericGetComponentMethod = getComponentMethod.MakeGenericMethod(componentDataType);
                if (genericGetComponentMethod.Invoke(itemData, null) is not ItemComponentData itemComponentData)
                {
                    throw new InvalidOperationException($"ItemData does not have component {componentTypeString}.");
                }
                
                var component = itemComponentData.CreateInstance();
                serializer.Populate(componentObject.CreateReader(), component);
                components.Add(component);
            }

            // var components = jsonObject["components"]?.ToObject<List<ItemComponent>>(serializer);
            Item item = new(itemData, components);

            return item;
        }
        
        public override void WriteJson(JsonWriter writer, Item value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public static class DataDeserializer
    {
        
        public static IEnumerable<ItemData> DeserializeItemData(string json)
        {
            return JsonConvert.DeserializeObject<ItemData[]>(json,
                new JsonSerializerSettings
                    { TypeNameHandling = TypeNameHandling.Auto, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
        
        public static Item DeserializeItem(string json, ItemConverter converter)
        {
            var settings = new JsonSerializerSettings
                { TypeNameHandling = TypeNameHandling.Auto, ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            settings.Converters.Add(converter);
            
            return JsonConvert.DeserializeObject<Item>(json, settings);
        }

        public static IEnumerable<CraftingRecipe> DeserializeRecipeData(string json)
        {
            return JsonConvert.DeserializeObject<CraftingRecipe[]>(json,
                new JsonSerializerSettings
                    { TypeNameHandling = TypeNameHandling.Auto, ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }
    }

    public class DataLoader : MonoBehaviour
    {
        public ItemRegistryObject itemRegistryObject;

        public CraftingRecipeRegistryObject recipeRegistryObject;

        private ItemRegistry _itemRegistry;
        private CraftingRecipeRegistry _recipeRegistry;

        private void Start()
        {
            _itemRegistry = itemRegistryObject.itemRegistry;
            _recipeRegistry = recipeRegistryObject.craftingRecipeRegistry;

            // Get JSON string from ItemData.json Asset
            TextAsset jsonAsset = Resources.Load<TextAsset>("ItemData");
            string json = jsonAsset.text;
            IEnumerable<ItemData> items = DataDeserializer.DeserializeItemData(json);

            foreach (ItemData item in items)
            {
                _itemRegistry.Add(item);
            }

            _itemRegistry.SetInitialized();

            TextAsset recipeAsset = Resources.Load<TextAsset>("RecipeData");
            string recipeJson = recipeAsset.text;
            IEnumerable<CraftingRecipe> recipes = DataDeserializer.DeserializeRecipeData(recipeJson);

            foreach (CraftingRecipe recipe in recipes)
            {
                _recipeRegistry.Add(recipe);
            }

            _recipeRegistry.SetInitialized();
        }
    }
}