using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Items
{
    [Serializable]
    public class PlaceableComponentData : ItemComponentData
    {
        public PlaceableComponentData(SerializableGameObject placedGameObject)
        {
            PlacedGameObject = placedGameObject;
            actions.Add(new ContextMenuAction("Place", Place));
        }

        [JsonProperty("placedGameObject")] public SerializableGameObject PlacedGameObject { get; }

        private void Place()
        {
            throw new NotImplementedException();
        }

        public override ItemComponent CreateInstance()
        {
            return new PlaceableComponent(this);
        }
    }

    public class PlaceableComponent : ItemComponent
    {
        public PlaceableComponent(PlaceableComponentData placeableComponentData) : base(placeableComponentData)
        {
        }
    }

    [Serializable]
    public class SerializableGameObject
    {
        public SerializableGameObject(string name, Vector3 position, Quaternion rotation)
        {
            Name = name;
            Position = position;
            Rotation = rotation;

            LoadedObject = Resources.Load<GameObject>(name);
        }

        [JsonProperty("name")] public string Name { get; }

        [JsonProperty("position")] public Vector3 Position { get; }

        [JsonProperty("rotation")] public Quaternion Rotation { get; }

        [JsonIgnore] public GameObject LoadedObject { get; }
    }
}