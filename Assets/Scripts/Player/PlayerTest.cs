using System;
using System.Collections;
using Crafting;
using DataManager;
using Inventory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Player
{
    [Serializable]
    public class PlayerTest : Player, PlayerActions.ICraftingTestActions
    {
        public MachineType machineType;
        public UnityEvent<MachineType> OnCraftEvent = new();
        private ItemRegistry _itemRegistry;

        private ItemRegistryObject _itemRegistryObject;
        private PlayerActions _playerActions;
        public PlayerInventory playerInventory = new("Mock Inventory");

        public void Start()
        {
            _itemRegistryObject = GameObject.Find("DataManager").GetComponent<ItemRegistryObject>();
            _itemRegistry = _itemRegistryObject.itemRegistry;

            StartCoroutine(GiveItems());
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

        private IEnumerator GiveItems()
        {
            if (!_itemRegistry.Initialized)
            {
                yield return new WaitUntil(() => _itemRegistry.Initialized);
            }

            // playerInventory.items.Add(_itemRegistry.Get(0x55518A64).CreateInstance());
            // playerInventory.items.Add(_itemRegistry.Get(0x55518A64).CreateInstance());
            // playerInventory.items.Add(_itemRegistry.Get(0x55518A64).CreateInstance());
            // playerInventory.items.Add(_itemRegistry.Get(0x238E2A2D).CreateInstance());
            // playerInventory.items.Add(_itemRegistry.Get(0x2E79821C).CreateInstance());
            // playerInventory.items.Add(_itemRegistry.Get(0x755CFE42).CreateInstance());
            // playerInventory.items.Add(_itemRegistry.Get(0xE3847C27).CreateInstance());

            Debug.Log("Gave items");
        }

        public override IInventory GetInventory()
        {
            return playerInventory;
        }

        public override IInventoryNotifier GetInventoryNotifier()
        {
            return playerInventory;
        }
    }
}