using Inventory;
using System;
using UnityEngine;

namespace Player
{
    public abstract class Player : MonoBehaviour
    {
        public abstract PlayerInventory GetInventory();
        public abstract void SetInventory(PlayerInventory inventory);
        public abstract IInventoryNotifier GetInventoryNotifier();

        internal void Place()
        {
            Debug.Log("Reached player's placing");
            throw new NotImplementedException();
        }
    }
}