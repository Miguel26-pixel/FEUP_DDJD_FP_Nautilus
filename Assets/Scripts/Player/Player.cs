using Inventory;
using UnityEngine;

namespace Player
{
    public abstract class Player : MonoBehaviour
    {
        public abstract IInventory GetInventory();
        public abstract IInventoryNotifier GetInventoryNotifier();
    }
}