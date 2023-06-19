using System;
using System.Collections;
using System.Collections.Generic;
using Crafting;
using DataManager;
using Inventory;
using UI.Inventory;
using UI.Inventory.Builders;
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
        public UnityEvent onPlacingStateChanged = new();
        public UnityEvent<int> onRotate = new();
        private ItemRegistry _itemRegistry;
        public TransferDirection transferDirection;

        private ItemRegistryObject _itemRegistryObject;
        private PlayerActions _playerActions;
        private bool _isPlacing = false;
        private bool _canPlaceObject = true;
        private GameObject _placingObject = null;

        public PlayerInventory playerInventory = new("Inventory", new[,]
        {
            { false, false, false, false, false, false },
            { false, false, false, false, false, false },
            { false, true, true, true, true, false },
            { true, true, true, true, true, true },
            { true, true, true, true, true, true },
            { true, true, true, true, true, true },
            { true, true, true, true, true, true },
            { false, true, true, true, true, false },
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

        private void Update()
        {

            Vector3 mousePosition = Mouse.current.position.ReadValue();
            bool raycast = Physics.Raycast(Camera.main.ScreenPointToRay(mousePosition), out RaycastHit hit);

            if (_isPlacing)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame && raycast && _canPlaceObject)
                {
                    _isPlacing = false;
                    OnPlacingStateChanged();
                }
                else if (raycast)
                {
                    Vector3 closestPointOnFloor = FindPlacingPoint(hit);
                    if (closestPointOnFloor != Vector3.zero)
                    {
                        _placingObject.transform.position = closestPointOnFloor;
                    }
                }
            }
            else
            {
                if (raycast)
                {
                    if (hit.collider.gameObject.TryGetComponent<MachineComponent>(out var machine))
                    {
                        machineType = machine.GetMachineType();
                    } 
                    else 
                    {
                        machineType = (MachineType)(-1);
                    }
                }

            }
        }

        private Vector3 FindPlacingPoint(RaycastHit hit)
        {
            Collider collider = hit.collider;
            if (collider != null && collider != _placingObject.GetComponent<Collider>())
            {
                _canPlaceObject = collider.gameObject.layer != LayerMask.NameToLayer("Water");
                Vector3 contactPoint = hit.point;
                Vector3 placementPosition = contactPoint + Vector3.up * (_placingObject.transform.localScale.y * 0.5f + 0.05f);
                return placementPosition;
            }

            return Vector3.zero;
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

        public void OnRotateClockwise(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            onRotate.Invoke(-1);
        }

        public void OnRotateAntiClockwise(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            onRotate.Invoke(1);
        }

        public void OnPlacingStateChanged()
        {
            onPlacingStateChanged.Invoke();
        }

        private IEnumerator GiveItems()
        {
            if (!_itemRegistry.Initialized)
            {
                yield return new WaitUntil(() => _itemRegistry.Initialized);
            }

            playerInventory.AddItem(_itemRegistry.Get(0x55518A64).CreateInstance());
            playerInventory.AddItem(_itemRegistry.Get(0x55518A64).CreateInstance());
            playerInventory.AddItem(_itemRegistry.Get(0x55518A64).CreateInstance());
            playerInventory.AddItem(_itemRegistry.Get(0x238E2A2D).CreateInstance());
            playerInventory.AddItem(_itemRegistry.Get(0x2E79821C).CreateInstance());
            playerInventory.AddItem(_itemRegistry.Get(0x755CFE42).CreateInstance());
            playerInventory.AddItem(_itemRegistry.Get(0xE3847C27).CreateInstance());
            playerInventory.AddItem(_itemRegistry.Get(0xDEC31753).CreateInstance());
            playerInventory.AddItem(_itemRegistry.Get(0x5BFE8AE3).CreateInstance());
            playerInventory.AddItem(_itemRegistry.Get(0xFE3EC9B0).CreateInstance());
            playerInventory.AddItem(_itemRegistry.Get(0x5C5C52AF).CreateInstance());


            Debug.Log("Gave items");
        }

        public override PlayerInventory GetInventory()
        {
            return playerInventory;
        }

        public override void SetInventory(PlayerInventory inventory)
        {
            playerInventory = inventory;
        }

        public override IInventoryNotifier GetInventoryNotifier()
        {
            return playerInventory;
        }

        public override void Place(GameObject instance)
        {
            _placingObject = instance;
            _isPlacing = true;
        }
    }
}