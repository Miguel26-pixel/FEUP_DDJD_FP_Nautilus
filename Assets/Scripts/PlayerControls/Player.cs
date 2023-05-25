

using System.Collections;
using Crafting;
using DataManager;
using Inventory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : AbstractPlayer, PlayerActions.ICraftingTestActions
{
    PlayerMovementInput playerInput; 
    CharacterController characterController;
    Animator animator;
    Rigidbody playerRigidbody;
    public Image healthBar;

    // Movement
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    bool isMovementPressed;
    int direction;

    // Jumping
    bool isJumpPressed = false;
    float initialJumpVelocity;
    float maxJumpHeight = 1.0f;
    float maxJumpTime = 0.5f;
    bool isJumping = false;

    // Camera
    float rotationFactorPerFrame = 15.0f;
    Vector3 cameraRelativeMovement;

    // Gravity
    float gravity = -9.8f;
    float groundGravity = -.05f;

    //Life Points
    public int Health = 1000;
    public int maxhealth = 1000;
    
    private Camera mainCamera;

    [Header("References")]
    public Transform cam;
    public Transform attackPoint = null;
    public GameObject weapon;

    [Header("Attacking")]
    public float throwForce;
    public float throwUpwardForce;
    public float throwCooldown;

    private bool readyToThrow;
    
    public MachineType machineType;
    public UnityEvent<MachineType> OnCraftEvent = new();
    public UnityEvent onInventoryEvent = new();
    public UnityEvent<int> onRotate = new();
    private ItemRegistry _itemRegistry;
    public TransferDirection transferDirection;

    private ItemRegistryObject _itemRegistryObject;
    private PlayerActions _playerActions;

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

    private static readonly int Attack = Animator.StringToHash("Attack");

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
        playerInput.CharacterControls.Enable();
    }
    
    public bool isDead
    {
        get
        {
            return Health == 0;
        }
    }

    public void TakeDamage(int amount)
    {
        animator.SetTrigger("Damage1");
        Health -= amount;
        healthBar.fillAmount = Health/maxhealth;
        if (Health < 0)
        {
            Health = 0;
        }

        if (isDead)
        {
            animator.SetTrigger("Death");
        }
    }

    private void Awake()
    {
        playerInput = new PlayerMovementInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        readyToThrow = true;

        playerInput.CharacterControls.Move.started += onMovementInput;
        playerInput.CharacterControls.Move.canceled += onMovementInput;
        playerInput.CharacterControls.Move.performed += onMovementInput;
        playerInput.CharacterControls.Jump.started += onJump;
        playerInput.CharacterControls.Jump.canceled += onJump;

        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }

    private void onMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
        direction = currentMovementInput.x > 0 || currentMovementInput.y > 0 ? 1 : -1;
    }

    private void onJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
    }

    private void HandleAttack()
    {
        animator.SetTrigger(Attack);
    }

    private void HandleAnimation()
    {
        bool isWalking = animator.GetBool("isWalking");

        if (isMovementPressed && !isWalking)
        {
            animator.SetBool("isWalking", true);
            animator.SetInteger("Direction", direction);
        }

        else if (!isMovementPressed && isWalking)
        {
            animator.SetInteger("Direction", 0);
            animator.SetBool("isWalking", false);
        }
    }

    private void HandleRotation()
    {
        Vector3 positionToLookAt;

        positionToLookAt.x = cameraRelativeMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = cameraRelativeMovement.z;

        Quaternion currentRotation = transform.rotation;

        if (isMovementPressed)
        {
            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
        }
    }

    private void HandleGravity()
    {
        if (characterController.isGrounded)
        {
            currentMovement.y = groundGravity;
        } 
        else
        {
            currentMovement.y += gravity * Time.deltaTime;
        }
    }

    private void HandleJump()
    {
        if (!isJumping && characterController.isGrounded && isJumpPressed)
        {
            animator.SetTrigger("Jump");
            currentMovement.y = initialJumpVelocity;
        }
        else if (!isJumpPressed && isJumping && characterController.isGrounded)
        {
            isJumping = false;
        }
    }

    private void OnDisable()
    {
        playerInput.CharacterControls.Disable();
    }

    private Vector3 ConvertToCameraSpace(Vector3 vectorToRotate)
    {
        float currentY = vectorToRotate.y;
        Vector3 cameraForward = Camera.main.transform.forward.normalized;
        Vector3 cameraRight = Camera.main.transform.right.normalized;

        cameraForward.y = 0;
        cameraRight.y = 0;

        Vector3 cameraForwardZProduct = vectorToRotate.z * cameraForward;
        Vector3 cameraForwardXProduct = vectorToRotate.x * cameraRight;

        Vector3 result = cameraForwardXProduct + cameraForwardZProduct;
        result.y = currentY;
        return result;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            cameraRelativeMovement = ConvertToCameraSpace(currentMovement);
            HandleRotation();
            HandleAnimation();
            characterController.Move(cameraRelativeMovement * Time.deltaTime);
            HandleGravity();
            HandleJump();

            if (Input.GetMouseButtonDown(0))
            {

                if(readyToThrow){
                    Throw();
                }
    

                HandleAttack();
            }

            if (Input.GetMouseButtonDown(1))
            {
                animator.SetTrigger("Run");
            }
        }
    }

    private void Throw()
    {
        readyToThrow = false;

        GameObject spear = Instantiate(weapon, attackPoint.position, cam.rotation);

        Rigidbody projectileRb = spear.GetComponent<Rigidbody>();

        Vector3 forceToAdd = cam.transform.forward * throwForce + transform.up * throwUpwardForce;

        projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

        Invoke(nameof(ResetThrow), throwCooldown);

    }

    private void ResetThrow()
    {
        readyToThrow = true;
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
}