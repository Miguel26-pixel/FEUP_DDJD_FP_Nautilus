using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    PlayerInput playerInput;
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

    void FixedUpdate()
    {
        // Cast a ray downwards to detect the floor
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, Mathf.Infinity))
        {
            // Adjust the player's position and rotation to match the floor
            playerRigidbody.MovePosition(hit.point);
            playerRigidbody.MoveRotation(Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation);
        }
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
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerRigidbody = GetComponent<Rigidbody>();

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
        animator.SetTrigger("Attack");
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

    private void OnEnable()
    {
        playerInput.CharacterControls.Enable();
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

            if (Input.GetKeyDown("e"))
            {
                HandleAttack();
            }
        }
    }

}
