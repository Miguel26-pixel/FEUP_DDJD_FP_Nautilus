using System;
using System.Collections;
using System.Collections.Generic;
using Crafting;
using DataManager;
using FMODUnity;
using Generation.Resource;
using Inventory;
using Items;
using UI;
using UI.Inventory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PlayerControls
{
    public class Player : AbstractPlayer, PlayerActions.IHUDActions, PlayerActions.IToolActions
    {
        public Image hungerBar;
        public Image oxygenBar;

        public PlayerController playerController;

        [Header("Stats")]
        public int health = 1000;
        public int maxHealth = 1000;
        public float _hunger = 1000;
        public float _maxHunger = 1000;
        public float _oxygen = 1000;
        public float _maxOxygen = 1000;

        [Header("HUD Events")]
        public MachineType machineType;
        public TransferDirection transferDirection;
        public UnityEvent<MachineType> onCraftEvent = new();
        public UnityEvent onInventoryEvent = new();
        public UnityEvent<int> onRotate = new();
        public UnityEvent<PopupData> onPopup = new();
        public UnityEvent<ProgressData> onProgress = new();
        public UnityEvent onPlacingStateChanged = new();

        [Header("Equipment")]
        public GameObject leftFlipperObject;
        public GameObject rightFlipperObject;

        public GameObject oxygenTankObject;
        public GameObject abismalOxygenTankObject;
        public GameObject oxygenMaskObject;

        [Header("References")]
        public TerraformController terraformController;
        private ItemData _soilData;

        private ItemRegistry _itemRegistry;

        private ItemRegistryObject _itemRegistryObject;
        private PlayerActions _playerActions;

        private bool _lockTool;

        private bool _isPlacing = false;
        private bool _canPlaceObject = true;
        private GameObject _placingObject = null;
        private int _itemIDHash = 0;

        [SerializeField]
        private float _interactionDistance = 3f;
        private float _placementDistance = 10f;

        [Header("Stats decay")]
        [SerializeField]
        private float _defautHungerDecay = 1f;
        private float _currentHungerDecay;
        [SerializeField]
        private float _runningDecayMultiplier = 1.5f;
        [SerializeField]
        private float _oxygenDecay = 10f;
        [SerializeField]
        private float _healthDecay = 50f;

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
            Debug.Log(SystemInfo.supportsAsyncCompute);
            Debug.Log(SystemInfo.supportsAsyncGPUReadback);
            Debug.Log(SystemInfo.supportsComputeShaders);
            Cursor.lockState = CursorLockMode.Locked;
            _currentHungerDecay = _defautHungerDecay;

            // StartCoroutine(GiveItems());
        }


        public void OnEnable()
        {
            if (_playerActions == null)
            {
                _playerActions = new PlayerActions();
                _playerActions.HUD.SetCallbacks(this);
                _playerActions.Tool.SetCallbacks(this);
            }

            _playerActions.HUD.Enable();
            _playerActions.Tool.Enable();
        }

        private void OnDisable()
        {
            _playerActions.HUD.Disable();
            _playerActions.Tool.Disable();
        }


        public bool IsDead => health == 0;
        public float HealthPercentage => health / (float)maxHealth;

        public void RemoveHealth(int amount)
        {
            health -= amount;
            if (health < 0)
            {
                health = 0;
            }
        }

        private void RemoveHunger(float amount)
        {
            float hungerPercentage = _hunger / _maxHunger;
            _hunger -= amount;
            if (_hunger < 0)
            {
                _hunger = 0;
            }
            hungerBar.fillAmount = hungerPercentage;
        }

        private void RemoveOxygen(float amount)
        {
            float oxygenPercentage = _oxygen / _maxOxygen;
            _oxygen -= amount;
            if (_oxygen < 0)
            {
                _oxygen = 0;
            }
            oxygenBar.fillAmount = oxygenPercentage;
        }

        private void RefillOxygen(float amount)
        {
            float oxygenPercentage = _oxygen / _maxOxygen;
            _oxygen += amount;
            if (_oxygen > _maxOxygen)
            {
                _oxygen = _maxOxygen;
            }
            oxygenBar.fillAmount = oxygenPercentage;
        }

        public void AddOxygenBoost(float boost)
        {
            _maxOxygen += boost * 10;
        }


        public void ResetHungerDecay()
        {
            _currentHungerDecay = _defautHungerDecay;
        }

        public void IncreaseHungerDecay()
        {
            _currentHungerDecay *= _runningDecayMultiplier;
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

        public void LockTool()
        {
            _lockTool = true;
            terraformController.DeactivateTerraform();
        }

        public void UnlockTool()
        {
            _lockTool = false;
        }

        private void Update()
        {
            HandleStats();

            Vector3 mousePosition = Mouse.current.position.ReadValue();
            bool raycast = Physics.Raycast(Camera.main.ScreenPointToRay(mousePosition), out RaycastHit hit);

            if (_isPlacing)
            {
                HandlePlacement(raycast, hit);
            }
            else
            {
                HandleInteraction();
            }
        }

        private Vector3 FindPlacingPoint(RaycastHit hit)
        {
            Collider collider = hit.collider;

            if (collider == null)
            {
                return Vector3.zero;
            }

            if (collider.gameObject.layer == LayerMask.NameToLayer("Vacuum") || collider == _placingObject.GetComponent<Collider>())
            {
                return Vector3.zero;
            }

            _canPlaceObject = collider.gameObject.layer != LayerMask.NameToLayer("Water");
            Vector3 contactPoint = hit.point;

            Quaternion rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            _placingObject.transform.rotation = rotation;

            float currentDistance = Vector3.Distance(transform.position, contactPoint);

            if (currentDistance <= _placementDistance)
            {
                return contactPoint;
            }
            else
            {
                Vector3 playerToPlacement = (contactPoint - transform.position).normalized;
                Vector3 closestPosition = transform.position + playerToPlacement * _placementDistance;
                return closestPosition;
            }

        }

        private MachineComponent FindClosestInteractibleMachine()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _interactionDistance);

            MachineComponent nearestMachine = null;
            float nearestDistance = float.MaxValue;

            foreach (Collider collider in colliders)
            {
                if (collider.TryGetComponent<MachineComponent>(out var machine))
                {
                    float distance = Vector3.Distance(transform.position, machine.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestMachine = machine;
                    }
                }
            }

            return nearestMachine;
        }

        private void HandleStats()
        {
            RemoveHunger(_currentHungerDecay * Time.deltaTime);

            if (IsDead)
            {
                return;
            }
            
            if (_hunger / _maxHunger < 0.05 || _oxygen / _maxOxygen < 0.05)
            {
                playerController.TakeDamage((int)(_healthDecay * Time.deltaTime));
            }

            if (playerController.underWater)
            {
                RemoveOxygen(_oxygenDecay * Time.deltaTime);
            }
            else if (_oxygen < _maxOxygen)
            {
                RefillOxygen(_oxygenDecay * 2 * Time.deltaTime);
            }
        }

        private void HandlePlacement(bool raycast, RaycastHit hit)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame && raycast && _canPlaceObject)
            {
                playerInventory.RemoveItem(_itemIDHash);
                RuntimeManager.PlayOneShot("event:/Player/Place machine");
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

        private void HandleInteraction()
        {
            MachineComponent nearestMachine = FindClosestInteractibleMachine();
            machineType = nearestMachine != null ? nearestMachine.GetMachineType() : MachineType.PocketFabricator;
        }

        public void OnCraft(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }
            onCraftEvent.Invoke(machineType);
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
            if (_lockTool)
            {
                return;
            }

            if (context.performed)
            {
                terraformController.ToggleTerraform();
            }
        }

        public void OnPlacingStateChanged()
        {
            onPlacingStateChanged.Invoke();
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

            SoilResource soilResource = playerInventory.AddSoil(_soilData, amount);

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

        private IEnumerator GiveItems()
        {
            if (!_itemRegistry.Initialized)
            {
                yield return new WaitUntil(() => _itemRegistry.Initialized);
            }

            playerInventory.AddItem(_itemRegistry.Get(0xBCFDBC37).CreateInstance());
            playerInventory.AddItem(_itemRegistry.Get(0x09B53F18).CreateInstance());
            playerInventory.AddItem(_itemRegistry.Get(0x5BFE8AE3).CreateInstance());
            playerInventory.AddItem(_itemRegistry.Get(0xE2042BBB).CreateInstance());
            playerInventory.AddItem(_itemRegistry.Get(0xFA0A52EE).CreateInstance());

            Debug.Log("Gave items");
        }

        public override void Place(GameObject instance, Item item)
        {
            _placingObject = instance;
            _isPlacing = true;
            _itemIDHash = item.IDHash;
        }

        private void EquipFlippers()
        {
            leftFlipperObject.SetActive(true);
            rightFlipperObject.SetActive(true);
        }

        private void UnequipFlippers()
        {
            leftFlipperObject.SetActive(false);
            rightFlipperObject.SetActive(false);
        }

        private void EquipOxygenTank()
        {
            oxygenTankObject.SetActive(true);
            oxygenMaskObject.SetActive(true);
        }

        private void UnequipOxygenTank()
        {
            oxygenTankObject.SetActive(false);
            oxygenMaskObject.SetActive(abismalOxygenTankObject.activeSelf);
        }

        private void EquipAbismalOxygenTank()
        {
            abismalOxygenTankObject.SetActive(true);
            oxygenMaskObject.SetActive(true);
        }

        private void UnequipAbismalOxygenTank()
        {
            abismalOxygenTankObject.SetActive(false);
            oxygenMaskObject.SetActive(oxygenTankObject.activeSelf);
        }

        public void EquipEquipment(Item item)
        {
            RuntimeManager.PlayOneShot("event:/Player/Equip sound (rustling)");

            switch (item.Name)
            {
                case "Flippers":
                    EquipFlippers();
                    break;
                case "Oxygen Tank":
                    EquipOxygenTank();
                    break;
                case "Abyssal Tank":
                    EquipAbismalOxygenTank();
                    break;
                default:
                    throw new NotImplementedException();
            }
            // _playerInventory.RemoveItem(item.IDHash);
        }

        public void UnequipEquipment(Item item)
        {
            RuntimeManager.PlayOneShot("event:/Player/Equip sound (rustling)");

            switch (item.Name)
            {
                case "Flippers":
                    UnequipFlippers();
                    break;
                case "Oxygen Tank":
                    UnequipOxygenTank();
                    break;
                case "Abyssal Tank":
                    UnequipAbismalOxygenTank();
                    break;
                default:
                    throw new NotImplementedException();
            }
            // _playerInventory.AddItem(item);
        }

        public void AddHunger(int hunger)
        {
            _hunger = Mathf.Clamp(_hunger + hunger, 0, _maxHunger);
        }

        public void AddHealth(int i)
        {
            health = Mathf.Clamp(health + i, 0, maxHealth);
        }
    }
}