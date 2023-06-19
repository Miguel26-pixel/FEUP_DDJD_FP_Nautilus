using Generation.Resource;
using Inventory;
using UnityEngine;

namespace PlayerControls
{
    public abstract class AbstractPlayer : MonoBehaviour
    {
        public abstract PlayerInventory GetInventory();
        public abstract void SetInventory(PlayerInventory inventory);
        public abstract IInventoryNotifier GetInventoryNotifier();
        public abstract bool CollectResource(Resource resource);
        public abstract void CollectSoil(float amount);
        public abstract bool RemoveSoil(float amount);
        public abstract void Place(GameObject instance);
    }
}