using System;
using System.Collections.Generic;
using Items;
using UnityEngine;

namespace Player
{
    public abstract class Player : MonoBehaviour
    {
        public abstract InventoryMock GetInventory();
    }
}