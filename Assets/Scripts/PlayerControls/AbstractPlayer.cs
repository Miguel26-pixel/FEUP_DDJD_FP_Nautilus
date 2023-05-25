using Inventory;

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public abstract class AbstractPlayer : MonoBehaviour
{
    public abstract PlayerInventory GetInventory();
    public abstract void SetInventory(PlayerInventory inventory);
    public abstract IInventoryNotifier GetInventoryNotifier();
}