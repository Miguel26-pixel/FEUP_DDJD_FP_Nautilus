using System;
using System.Collections;
using Crafting;
using DataManager;
using Generation.Resource;
using Inventory;
using Items;
using UI;
using UI.Inventory;
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
        public UnityEvent<int> onRotate = new();
        public UnityEvent<PopupData> onPopup = new();
        public UnityEvent<ProgressData> onProgress = new();
        public TransferDirection transferDirection;
        public TerraformController terraformController;
        private Camera _camera;
        private Vector3 _currentMovement;
        private float _horizontalRotation;
        private ItemData _soilData;

        private ItemRegistry _itemRegistry;

        private ItemRegistryObject _itemRegistryObject;
        private bool _movementLocked;
        private PlayerActions _playerActions;
        private Rigidbody _rigidbody;
        private float _verticalRotation;

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
            _camera = Camera.main;
            _rigidbody = GetComponent<Rigidbody>();
            Debug.Log(SystemInfo.supportsAsyncCompute);
            Debug.Log(SystemInfo.supportsAsyncGPUReadback);
            Debug.Log(SystemInfo.supportsComputeShaders);
            Cursor.lockState = CursorLockMode.Locked;

            StartCoroutine(GiveItems());
        }

        private void Update()
        {
            if (_movementLocked)
            {
                return;
            }

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            transform.Rotate(Vector3.up * (mouseDelta.x * 0.1f), Space.World);

            _verticalRotation -= mouseDelta.y * 0.1f;
            _verticalRotation = Mathf.Clamp(_verticalRotation, -90f, 90f);
            _horizontalRotation += mouseDelta.x * 0.1f;

            _camera.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0f);
            terraformController.vacuumArea.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0f);
            terraformController.vacuumCollection.transform.localRotation = Quaternion.Euler(_verticalRotation, 0, 0f);

            if (_currentMovement != Vector3.zero)
            {
                Vector3 forward = _camera.transform.forward;
                forward.Normalize();

                _rigidbody.velocity = forward * (_currentMovement.x * 20f);
            }
            else
            {
                _rigidbody.velocity = Vector3.zero;
            }
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

        public void OnUseTool(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                terraformController.SetTerraformType(TerraformType.Lower);
            }
            else if (context.canceled)
            {
                terraformController.SetTerraformType(TerraformType.None);
            }
        }

        public void OnUseRightTool(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                terraformController.SetTerraformType(TerraformType.Raise);
            }
            else if (context.canceled)
            {
                terraformController.SetTerraformType(TerraformType.None);
            }
        }

        public void OnEquipTool(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                terraformController.ToggleTerraform();
            }
        }

        public void OnForward(InputAction.CallbackContext context)
        {
            // Move in the direction of the camera
            if (context.performed)
            {
                _currentMovement = new Vector3(1, 1, 1);
            }
            else if (context.canceled)
            {
                _currentMovement = Vector3.zero;
            }
        }

        public override void LockMovement()
        {
            _movementLocked = true;
            
            _currentMovement = Vector3.zero;
        }

        public override void UnlockMovement()
        {
            _movementLocked = false;
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
            // playerInventory.AddItem(_itemRegistry.Get(0x5C5C52AF).CreateInstance());


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
        
        public override bool RemoveSoil(float amount)
        {
            _soilData ??= _itemRegistry.Get(0x6F9576E5);

            bool removed = playerInventory.RemoveSoil(_soilData, amount);

            if (!removed)
            {
                onPopup.Invoke(new PopupData("Not enough soil", IconRepository.IconType.Error));
                return false;
            }
            
            SoilResource soilResource = playerInventory.GetSoilResource();

            // This should never be null, but just in case
            if (soilResource != null)
            {
                onProgress.Invoke(new ProgressData(_soilData.Name, _soilData.Icon, soilResource.Count,
                    soilResource.MaxCount, soilResource.Item));
            }

            return true;
        }

        public override void CollectSoil(float amount)
        {
            _soilData ??= _itemRegistry.Get(0x6F9576E5);
            
            SoilResource soilResource  = playerInventory.AddSoil(_soilData, amount);

            if (soilResource == null)
            {
                return;
            }
            
            onProgress.Invoke(new ProgressData(_soilData.Name, _soilData.Icon, soilResource.Count,
                soilResource.MaxCount, soilResource.Item));
        }

        public override bool CollectResource(Resource resource)
        {
            if (string.IsNullOrEmpty(resource.itemHash))
            {
                return false;
            }

            ItemData item = _itemRegistry.Get(resource.itemID);
            IntermediateResource intermediateResource = playerInventory.AddResource(item);

            if (intermediateResource != null)
            {
                Destroy(resource.gameObject);
                
                onProgress.Invoke(new ProgressData(item.Name, item.Icon, intermediateResource.Count,
                    intermediateResource.NeededCollectionCount, intermediateResource.Item));
            }
            else
            {
                onPopup.Invoke(new PopupData("Inventory full", IconRepository.IconType.Error));
            }

            return intermediateResource != null;
        }
    }
}