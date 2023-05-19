using Inventory;
using UnityEngine;

namespace Player
{
    public abstract class Player : MonoBehaviour
    {
        public abstract PlayerInventory GetInventory();
        public abstract IInventoryNotifier GetInventoryNotifier();
    }
}