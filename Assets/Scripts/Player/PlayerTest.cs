using System;
using System.Collections;
using System.Collections.Generic;
using Crafting;
using DataManager;
using Items;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Player
{
    [Serializable]
    public class PlayerTest : Player, PlayerActions.ICraftingTestActions
    {
        private PlayerActions _playerActions;
        public MachineType machineType;
        public InventoryMock inventoryMock = new();
        public UnityEvent<MachineType> OnCraftEvent = new();

        private ItemRegistryObject _itemRegistryObject;
        private ItemRegistry _itemRegistry;

        public void Start()
        {
            _itemRegistryObject = GameObject.Find("DataManager").GetComponent<ItemRegistryObject>();
            _itemRegistry = _itemRegistryObject.itemRegistry;

            StartCoroutine(GiveItems());
        }

        private IEnumerator GiveItems()
        {
            if (!_itemRegistry.Initialized)
            {
                yield return new WaitUntil(() => _itemRegistry.Initialized);

            }
            
            inventoryMock.items.Add(_itemRegistry.Get(0x55518A64).CreateInstance());
            inventoryMock.items.Add(_itemRegistry.Get(0x55518A64).CreateInstance());
            inventoryMock.items.Add(_itemRegistry.Get(0x55518A64).CreateInstance());
            inventoryMock.items.Add(_itemRegistry.Get(0x238E2A2D).CreateInstance());
            inventoryMock.items.Add(_itemRegistry.Get(0x2E79821C).CreateInstance());
            inventoryMock.items.Add(_itemRegistry.Get(0x755CFE42).CreateInstance());
            inventoryMock.items.Add(_itemRegistry.Get(0xE3847C27).CreateInstance());
            
            Debug.Log("Gave items");
        }

        public void OnEnable()
        {
            if (_playerActions == null)
            {
                _playerActions = new PlayerActions();
                _playerActions.CraftingTest.SetCallbacks(this);
            }
            
            _playerActions.CraftingTest.Enable();
        }

        public void OnDisable()
        {
            _playerActions.CraftingTest.Disable();
        }


        public void OnCraft(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            Debug.Log("Craft");
            OnCraftEvent.Invoke(machineType);
        }

        public override InventoryMock GetInventory()
        {
            return inventoryMock;
        }
    }
}