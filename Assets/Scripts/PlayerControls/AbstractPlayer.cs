using Inventory;
using UnityEngine;

namespace PlayerControls
{
    public abstract class AbstractPlayer : MonoBehaviour
    {
        public abstract PlayerInventory GetInventory();
        public abstract void SetInventory(PlayerInventory inventory);
        public abstract IInventoryNotifier GetInventoryNotifier();
    }
}