using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Items
{
    public class PlaceableComponent : ItemComponent
    {
        [JsonProperty("placedGameObject")]
        public SerializableGameObject PlacedGameObject { get; }

        public PlaceableComponent(SerializableGameObject placedGameObject)
        {
            PlacedGameObject = placedGameObject;
            actions.Add(new ContextMenuAction("Place", Place));
        }

        private void Place()
        {
            throw new System.NotImplementedException();
        }
    }

    [Serializable]
    public class SerializableGameObject
    {
        [JsonProperty("name")]
        public string Name { get; }
        [JsonProperty("position")]
        public Vector3 Position { get; }
        [JsonProperty("rotation")]
        public Quaternion Rotation { get; }
        
        public SerializableGameObject(string name, Vector3 position, Quaternion rotation)
        {
            Name = name;
            Position = position;
            Rotation = rotation;
        }
    }
}