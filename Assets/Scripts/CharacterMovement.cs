using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    PlayerInput playerInput;
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    bool isMovementPressed;

    CharacterController characterController;
    Animator animator;

    float rotationFactorPerFrame = 15.0f;

    Vector3 cameraRelativeMovement;

    private void Awake()
    {
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        playerInput.CharacterControls.Move.started += onMovementInput;
        playerInput.CharacterControls.Move.canceled += onMovementInput;
        playerInput.CharacterControls.Move.performed += onMovementInput;
    }

    private void onMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        currentMovement.x = currentMovementInput.x;
        currentMovement.z = currentMovementInput.y;
        isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
    }

    private void HandleAnimation()
    {
        bool isWalking = animator.GetBool("isWalking");

        if (isMovementPressed && !isWalking)
        {
            animator.SetBool("isWalking", true);
        }

        else if (!isMovementPressed && isWalking)
        {
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
            float groundedGravity = -.5f;
            currentMovement.y = groundedGravity;
        } 
        else
        {
            float gravity = -9.8f;
            currentMovement.y += gravity;
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
        cameraRelativeMovement = ConvertToCameraSpace(currentMovement);
        HandleRotation();
        HandleAnimation();
        HandleGravity();
        characterController.Move(cameraRelativeMovement * Time.deltaTime);

    }

}
