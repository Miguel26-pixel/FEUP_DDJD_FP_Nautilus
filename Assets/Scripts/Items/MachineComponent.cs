using Crafting;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineComponent : MonoBehaviour
{
    [SerializeField]
    private MachineType _machineType;

    public MachineType GetMachineType()
    {
        return _machineType;
    }

}
