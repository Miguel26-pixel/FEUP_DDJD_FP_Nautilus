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
        [JsonConstructor]
        private SerializableGameObject(string name, SerializableVector3 position, SerializableQuaternion rotation)
        {
            Name = name;
            Position = new Vector3(position.X, position.Y, position.Z);
            Rotation = new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);

            LoadedObject = Resources.Load<GameObject>(name);
        }

        public SerializableGameObject(string name, Vector3 position, Quaternion rotation)
        {
            Name = name;
            Position = position;
            Rotation = rotation;

            LoadedObject = Resources.Load<GameObject>(name);
        }

        [JsonProperty("name")] public string Name { get; }

        [JsonIgnore] public Vector3 Position { get; }
        [JsonProperty("position")] private SerializableVector3 PositionSerialized => new(Position);

        [JsonIgnore] public Quaternion Rotation { get; }
        [JsonProperty("rotation")] private SerializableQuaternion RotationSerialized => new(Rotation);

        [JsonIgnore] public GameObject LoadedObject { get; }

        private class SerializableVector3
        {
            [JsonConstructor]
            public SerializableVector3(float x, float y, float z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public SerializableVector3(Vector3 vector3)
            {
                X = vector3.x;
                Y = vector3.y;
                Z = vector3.z;
            }

            [JsonProperty("x")] public float X { get; }
            [JsonProperty("y")] public float Y { get; }
            [JsonProperty("z")] public float Z { get; }
        }


        private class SerializableQuaternion
        {
            [JsonConstructor]
            public SerializableQuaternion(float x, float y, float z, float w)
            {
                X = x;
                Y = y;
                Z = z;
                W = w;
            }

            public SerializableQuaternion(Quaternion quaternion)
            {
                X = quaternion.x;
                Y = quaternion.y;
                Z = quaternion.z;
                W = quaternion.w;
            }

            [JsonProperty("x")] public float X { get; }
            [JsonProperty("y")] public float Y { get; }
            [JsonProperty("z")] public float Z { get; }
            [JsonProperty("w")] public float W { get; }
        }
    }
}