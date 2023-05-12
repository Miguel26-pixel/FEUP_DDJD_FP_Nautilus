using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Items
{
    [Serializable]
    public class PlaceableComponent : ItemComponent
    {
        public PlaceableComponent(SerializableGameObject placedGameObject)
        {
            PlacedGameObject = placedGameObject;
            actions.Add(new ContextMenuAction("Place", Place));
        }

        [JsonProperty("placedGameObject")] public SerializableGameObject PlacedGameObject { get; }

        private void Place()
        {
            throw new NotImplementedException();
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