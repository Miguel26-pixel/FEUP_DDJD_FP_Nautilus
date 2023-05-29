using Generation.Resource;
using Inventory;
using UnityEngine;

namespace Player
{
    public abstract class Player : MonoBehaviour
    {
        public abstract PlayerInventory GetInventory();
        public abstract void SetInventory(PlayerInventory inventory);
        public abstract IInventoryNotifier GetInventoryNotifier();
        public abstract bool CollectResource(Resource resource);
        public abstract void CollectSoil(float amount);
        public abstract void LockMovement();
        public abstract void UnlockMovement();
    }
}