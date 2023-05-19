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
        public UnityEvent onInventoryEvent = new();
        private ItemRegistry _itemRegistry;

        private ItemRegistryObject _itemRegistryObject;
        private PlayerActions _playerActions;

        public PlayerInventory playerInventory = new("Inventory", new[,]
        {
            { false, false, false, false, false, false },
            { false, false, false, false, false, false },
            { false, false, true, true, false, false },
            { false, true, true, true, true, false },
            { false, true, true, true, true, false },
            { false, true, true, true, true, false },
            { false, true, true, true, true, false },
            { false, false, true, true, false, false },
            { false, false, false, false, false, false }
        });

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

            OnCraftEvent.Invoke(machineType);
        }

        public void OnInventory(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            onInventoryEvent.Invoke();
        }

        private IEnumerator GiveItems()
        {
            if (!_itemRegistry.Initialized)
            {
                yield return new WaitUntil(() => _itemRegistry.Initialized);
            }

            // playerInventory.AddItem(_itemRegistry.Get(0x55518A64).CreateInstance());
            // playerInventory.AddItem(_itemRegistry.Get(0x55518A64).CreateInstance());
            // playerInventory.AddItem(_itemRegistry.Get(0x55518A64).CreateInstance());
            // playerInventory.AddItem(_itemRegistry.Get(0x238E2A2D).CreateInstance());
            // playerInventory.AddItem(_itemRegistry.Get(0x2E79821C).CreateInstance());
            // playerInventory.AddItem(_itemRegistry.Get(0x755CFE42).CreateInstance());
            // playerInventory.AddItem(_itemRegistry.Get(0xE3847C27).CreateInstance());
            
            playerInventory.AddItem(_itemRegistry.Get(0x5C5C52AF).CreateInstance(), new Vector2Int(1, 3), 4);
            
            
            Debug.Log("Gave items");
        }

        public override PlayerInventory GetInventory()
        {
            return playerInventory;
        }

        public override IInventoryNotifier GetInventoryNotifier()
        {
            return playerInventory;
        }
    }
}