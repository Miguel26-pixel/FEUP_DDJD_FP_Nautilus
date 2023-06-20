using System;
using System.Globalization;
using UnityEngine;

namespace Generation.Resource
{
    public class Resource : MonoBehaviour
    {
        public string itemHash;
        [NonSerialized] public bool dropped;
        [NonSerialized] public int itemID;

        public void Awake()
        {
            if (!string.IsNullOrEmpty(itemHash))
            {
                itemID = int.Parse(itemHash, NumberStyles.HexNumber);
            }
        }
    }
}