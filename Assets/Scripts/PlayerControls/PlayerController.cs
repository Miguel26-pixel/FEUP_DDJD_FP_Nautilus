using System;
using System.Collections;
using Crafting;
using DataManager;
using FMOD.Studio;
using FMODUnity;
using Inventory;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PlayerControls
{
    public enum JumpState
    {
        Start,
        Jumping,
        Apex,
        Landing,
        End
    }

    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Player))]
    public class PlayerController : MonoBehaviour, PlayerActions.IMovementActions
    {
        public Image healthBar;

        private CharacterController _characterController;
        private Animator _animator;
        private Player _player;

        // Movement
        [Header("Movement")]
        public float walkingSpeed = 6.0f;
        public float runningSpeed = 12.0f;
        public float swimmingSpeed = 3.0f;
        public float swimmingFastSpeed = 6.0f;

        public float minSpeed = 2.0f;
        public float maxSpeed = 15.0f;

        public float minSwimmingSpeed = 1.0f;
        public float maxSwimmingSpeed = 10.0f;
        
        [Range(-1f, 2f)]
        public float distanceToGround = 0.1f;
        public float animationToGround = 0.1f;

        public float legDistance = 33.76f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask waterLayer;

        private float _runningBoost = 0;
        private float _swimmingBoost = 0;

        private float _speed;
        private float _targetSpeed;
        private bool _isRunning;

        private Vector2 _currentMovementInput;
        private Vector3 _currentMovement;
        private bool _isMovementPressed;
        private bool _movementLocked;

        public void AddSpeedBoost(float boost)
        {
            _runningBoost += boost;
        }

        public void AddSwimmingBoost(float boost)
        {
            _swimmingBoost += boost;
        }
        
        
        public void RemoveRunningBoost(float boost)
        {
            _runningBoost -= boost;
        }
        
        // Jumping
        [Header("Jumping")]
        public float maxJumpHeight = 1.0f;
        public float maxJumpTime = 0.5f;

        private float _initialJumpVelocity;
        private Vector3 _jumpVelocity;
        private JumpState _jumpState = JumpState.End;
        private bool _isJumpPressed;

        // Camera
        private Camera _mainCamera;
        private Transform _cameraTransform;
        private Transform _cameraParentTransform;

        private const float rotationFactorPerFrame = 15.0f;
        private Vector3 _cameraRelativeMovement;

        // Gravity
        private float _gravity = Physics.gravity.y;
        private const float GroundGravity = -.5f;

        [Header("References")]
        public Transform attackPoint;
        public GameObject weapon;

        [Header("Attacking")]
        public float throwForce;
        public float throwUpwardForce;
        public float throwCooldown;

        private bool _readyToThrow;

        private PlayerActions _playerActions;

        private static readonly int Attack = Animator.StringToHash("Attack");
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int Grounded = Animator.StringToHash("Grounded");
        
        private int _jumpOverrideLayer;
        private float _ikWeight = 1;
        private float _landTime;
        private static readonly int JumpTime = Animator.StringToHash("JumpTime");

        [Header("Swim Event")]
        public float waterDistance = 1;
        
        public bool underWater = false;
        public float swimmSpeed = 0.0f;
        private static readonly int SpeedDifference = Animator.StringToHash("SpeedDifference");
        
        private string _stepReference;
        private string _swimReference;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
            _player = GetComponent<Player>();

            _readyToThrow = true;

            // _initialJumpVelocity = Mathf.Sqrt(-2f * _gravity * maxJumpHeight);

            float timeToApex = maxJumpTime;
            _gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
            _initialJumpVelocity = -_gravity * timeToApex;
                
            _jumpOverrideLayer = _animator.GetLayerIndex("Jump");

            RuntimeAnimatorController ac = _animator.runtimeAnimatorController;
            
            foreach (AnimationClip t in ac.animationClips)
            {
                switch (t.name)
                {
                    case "Jump":
                    {
                        float jumpStart = t.events[0].time;
                        _animator.SetFloat(JumpTime, timeToApex / (t.length - jumpStart));
                        break;
                    }
                    case "Land":
                        float landStart = t.events[0].time;
                        _landTime = landStart;
                        break;
                }
            }
        }

        public void Start()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                return;
            }

            _cameraTransform = _mainCamera.transform;
            _cameraParentTransform = _cameraTransform.parent;
            
            _stepReference = "event:/Player/Walking on sand";
            _swimReference = "event:/Player/Swimming";
        }

        public void OnEnable()
        {
            if (_playerActions == null)
            {
                _playerActions = new PlayerActions();
                _playerActions.Movement.SetCallbacks(this);
            }

            _playerActions.Movement.Enable();
        }
    
        private void OnDisable()
        {
            _playerActions.Movement.Disable();
        }

        public void LockMovement()
        {
            _movementLocked = true;
        }

        public void UnlockMovement()
        {
            _movementLocked = false;
        }
        
        private void Update()
        {
            if (_player.IsDead)
            {
                return;
            }
            
            if (_isMovementPressed && !_movementLocked)
            {
                _targetSpeed = underWater
                    ? Mathf.Clamp((_isRunning ? swimmingFastSpeed : swimmingSpeed) + _swimmingBoost, minSwimmingSpeed, maxSwimmingSpeed)
                    : Mathf.Clamp((_isRunning ? runningSpeed : walkingSpeed) + _runningBoost, minSpeed, maxSpeed);
            }
            else
            {
                _targetSpeed = 0.0f;
            }

            _speed = Math.Abs(_speed - _targetSpeed) > 0.01f
                ? Mathf.Lerp(_speed, _targetSpeed, Time.deltaTime * 10.0f)
                : _targetSpeed;

            
            HandleWater();
            _cameraRelativeMovement = ConvertToCameraSpace(_currentMovement);

            if (underWater && 19.5f > transform.position.y)
            {
                _cameraRelativeMovement.y = -((0.3f + Mathf.Clamp(_cameraTransform.localRotation.x, -0.3f, 0.3f)) / 0.6f * 3f - 1.5f);
            }

            HandleRotation();
            HandleAnimation();
            HandleGravity();
            _characterController.Move(_cameraRelativeMovement * (Time.deltaTime * _speed) + _jumpVelocity * Time.deltaTime);
            HandleJump();
        }


        public void TakeDamage(int amount)
        {
            _animator.SetTrigger("Damage1");
            _player.RemoveHealth(amount);

            healthBar.fillAmount = _player.HealthPercentage;
            if (_player.IsDead)
            {
                _animator.SetTrigger("Death");
            }
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            _currentMovementInput = context.ReadValue<Vector2>();
            _currentMovement.x = _currentMovementInput.x;
            _currentMovement.z = _currentMovementInput.y;
            _isMovementPressed = _currentMovementInput.x != 0 || _currentMovementInput.y != 0;
        }

        public void OnRun(InputAction.CallbackContext context)
        {
            if (context.performed)
            { 
                _isRunning = true;
            }
            else if(context.canceled)
            {
                _isRunning = false;
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (_jumpState != JumpState.End)
            {
                return;
            }
            
            _isJumpPressed = context.ReadValueAsButton();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (!context.performed)
            {
                return;
            }

            if(!_readyToThrow)
            {
                return;
            }
        
            Throw();
            _animator.SetTrigger(Attack);
        }

        private void HandleWater()
        {
            if (Physics.Raycast(transform.position, Vector3.up, out var hit, 1000f, waterLayer))
            {
                float distance = Mathf.Clamp01((hit.point.y - transform.position.y) / waterDistance);
                underWater = true;

                _animator.SetFloat("WaterDistance", distance);
            }
            else
            {
                underWater = false;
                _animator.SetFloat("WaterDistance", 0f);
            }
        }

        private void HandleAnimation()
        {
            float animSpeed;
            float minSpeedMultiplier = 0.7f;
            float maxSpeedMultiplier = 1.3f;
            float normalSpeed = !underWater ? walkingSpeed : swimmingSpeed;
            float fastSpeed = !underWater ? runningSpeed : swimmingFastSpeed;
            float envMinSpeed = !underWater ? minSpeed : minSwimmingSpeed;
            float envMaxSpeed = !underWater ? maxSpeed : maxSwimmingSpeed;
            float minBoost = envMinSpeed - normalSpeed;
            float maxBoost = envMaxSpeed - fastSpeed;
            float boost = !underWater ? _runningBoost : _swimmingBoost;

            float speedDifferenceMultiplier = (Mathf.Clamp(boost, minBoost, maxBoost) - minBoost) / (maxBoost - minBoost) * (maxSpeedMultiplier - minSpeedMultiplier) +
                                              minSpeedMultiplier;

            float modifiedNormalSpeed = Mathf.Clamp(normalSpeed + boost, envMinSpeed, envMaxSpeed);
            float modifiedFastSpeed = Mathf.Clamp(fastSpeed + boost, envMaxSpeed, envMaxSpeed);

            _animator.SetFloat(SpeedDifference, speedDifferenceMultiplier);

            if (_speed <= modifiedNormalSpeed)
            {
                animSpeed = _speed / modifiedNormalSpeed;
            }
            else
            {
                animSpeed = (_speed - modifiedNormalSpeed) / (modifiedFastSpeed - modifiedNormalSpeed) + 1;
            }

            _animator.SetFloat(Speed, animSpeed);
            _animator.SetBool(Grounded, _characterController.isGrounded);
        }

        private void HandleRotation()
        {
            if (_movementLocked)
            {
                return;
            }
            
            Vector3 positionToLookAt;

            positionToLookAt.x = _cameraRelativeMovement.x;
            positionToLookAt.y = 0.0f;
            if (underWater)
            {
                positionToLookAt.y = -((0.3f + Mathf.Clamp(_cameraTransform.localRotation.x, -0.3f, 0.3f)) / 0.6f * 2f - 1f);
            }

            positionToLookAt.z = _cameraRelativeMovement.z;

            Quaternion currentRotation = transform.rotation;

            if (!_isMovementPressed)
            {
                return;
            }

            Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);

            transform.rotation = 1 - Mathf.Abs(Quaternion.Dot(currentRotation, targetRotation)) < 0.01f
                ? targetRotation
                : Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
        }

        private void HandleGravity()
        {
            if (_characterController.isGrounded && !underWater)
            {
                _jumpVelocity.y = GroundGravity;
                return;
            }

            if(underWater)
            {
                _jumpVelocity.y = 0.0f;
                _gravity = 0.0f;
                return;
            }
            _jumpVelocity.y += _gravity * Time.deltaTime;

        }

        private void HandleJump()
        {
            switch (_jumpState)
            {
                case JumpState.End:
                    if (_characterController.isGrounded && _isJumpPressed && !_movementLocked)
                    {
                        _animator.SetTrigger(Jump);
                        _animator.SetLayerWeight(_jumpOverrideLayer, 0.35f);
                        
                        _isJumpPressed = false;
                        _jumpState = JumpState.Start;
                    }
                    break;
                case JumpState.Start:
                    break;
                case JumpState.Jumping:
                    break;
                case JumpState.Apex:
                    if ( _characterController.isGrounded)
                    {
                        _jumpState = JumpState.End;
                        _ikWeight = 1f;
                    }

                    if (Physics.Raycast(transform.position, Vector3.down, out var hit, 10f, groundLayer))
                    {
                        float distance = transform.position.y - hit.point.y;
                        float velocity = -_jumpVelocity.y;
                        float gravity = -_gravity;
                        float timeToGround = (Mathf.Sqrt(2 * gravity * distance + velocity * velocity) - velocity) / gravity;

                        if (timeToGround < _landTime)
                        {
                            float startTime = _landTime - timeToGround;
                            // start landing animation
                            _animator.CrossFadeInFixedTime("Land", 0.1f, _jumpOverrideLayer, startTime);
                            _jumpState = JumpState.Landing;
                            StartCoroutine(BlendJumpLayers(0.35f, JumpState.Landing));
                        }
                    }
                    
                    break;
                case JumpState.Landing:
                    if ( _characterController.isGrounded)
                    {
                        _jumpState = JumpState.End;
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void LiftJump()
        {
            if(_jumpState != JumpState.Start)
            {
                return;
            }
            
            _jumpState = JumpState.Jumping;
            _jumpVelocity.y = _initialJumpVelocity;
            
            StartCoroutine(BlendJumpLayers(1f, JumpState.Jumping));
        }
        
        public void ApexJump()
        {
            if (_jumpState != JumpState.Jumping)
            {
                return;
            }
            
            _jumpState = JumpState.Apex;
        }

        private IEnumerator BlendJumpLayers(float endOverride, JumpState state)
        {
            float startOverride = _animator.GetLayerWeight(_jumpOverrideLayer);

            float t = 0f;

            while (t < 1f && _jumpState == state)
            {
                t += Time.deltaTime * 9f;
                float currentBlendOverride = Mathf.Lerp(startOverride, endOverride, t);

                _animator.SetLayerWeight(_jumpOverrideLayer, currentBlendOverride);

                yield return null;
            }
        }
    
        private Vector3 ConvertToCameraSpace(Vector3 vectorToRotate)
        {
            if (_movementLocked)
            {
                return Vector3.zero;
            }
            
            Vector3 cameraForward = _cameraParentTransform.forward.normalized;
            Vector3 cameraRight = _cameraParentTransform.right.normalized;

            cameraForward.y = 0;
            cameraRight.y = 0;

            Vector3 cameraForwardZProduct = vectorToRotate.z * cameraForward;
            Vector3 cameraForwardXProduct = vectorToRotate.x * cameraRight;

            Vector3 result = cameraForwardXProduct + cameraForwardZProduct;

            return new Vector3(result.x, 0, result.z);
        }

        private void Throw()
        {
            if (_movementLocked)
            {
                return;
            }
            
            _readyToThrow = false;

            GameObject spear = Instantiate(weapon, attackPoint.position, _cameraTransform.rotation);

            Rigidbody projectileRb = spear.GetComponent<Rigidbody>();

            Vector3 forceToAdd = _cameraTransform.transform.forward * throwForce + transform.up * throwUpwardForce;

            projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

            Invoke(nameof(ResetThrow), throwCooldown);

        }

        private void ResetThrow()
        {
            _readyToThrow = true;
        }
        
        private float _lastLeftIK = 0f;
        private float _lastRightIK = 0f;
        private float _leftIK = 0f;
        private float _rightIK = 0f;

        private Vector3 _lastLeft;
        private Vector3 _lastRight;
        private bool leftMoving = false;

        private void OnAnimatorIK(int layerIndex)
        {
            const float maxDistance = 2.4f;
            
            var leftTransform = GetFootTransform(maxDistance, AvatarIKGoal.LeftFoot);
            var rightTransform = GetFootTransform(maxDistance, AvatarIKGoal.RightFoot);
            
            Ray ray = new(_animator.bodyPosition, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 10f, groundLayer))
            {
                _ikWeight = Mathf.Lerp(1f, 0f,
                    (hit.distance - legDistance - distanceToGround) / (distanceToGround / 1.5f));
            }
            else
            {
                _ikWeight = 0f;
            }

            LowerBody(leftTransform.Item1, rightTransform.Item1);
            
            SetFootTransform(AvatarIKGoal.LeftFoot, leftTransform.Item1, leftTransform.Item2);
            SetFootTransform(AvatarIKGoal.RightFoot, rightTransform.Item1, rightTransform.Item2);
            
            _leftIK = _animator.GetIKRotationWeight(AvatarIKGoal.LeftFoot);

            if (Math.Abs(_leftIK - 1) < 0.01f && Math.Abs(_lastLeftIK - 1) > 0.01f && _ikWeight > 0.8f)
            {
                RuntimeManager.PlayOneShot(_stepReference);
            }
            _lastLeftIK = _leftIK;
                    
            _rightIK = _animator.GetIKRotationWeight(AvatarIKGoal.RightFoot);
                    
            if (Math.Abs(_rightIK - 1) < 0.01f && Math.Abs(_lastRightIK - 1) > 0.01f && _ikWeight > 0.8f)
            {
                RuntimeManager.PlayOneShot(_stepReference);
            }
            _lastRightIK = _rightIK;

            var left = _animator.GetIKPosition(AvatarIKGoal.LeftFoot);
            var right = _animator.GetIKPosition(AvatarIKGoal.RightFoot);
            
            var forward = transform.forward;
            
            var leftComponent = Vector3.Dot(left - _lastLeft, forward) / forward.magnitude;
            var rightComponent = Vector3.Dot(right - _lastRight, forward) / forward.magnitude;

            
            if (transform.position.y <= 22f && ((leftComponent > rightComponent + 0.1f && !leftMoving) || (rightComponent > leftComponent + 0.1f && leftMoving)  ))
            {
                RuntimeManager.PlayOneShot(_swimReference);
                leftMoving = !leftMoving;
            }
            
            _lastLeft = left;
            _lastRight = right;
        }

        private Tuple<Vector3, Quaternion> GetFootTransform(float maxDistance, AvatarIKGoal goal)
        {
            Ray ray = new(_animator.GetIKPosition(goal) + Vector3.up, Vector3.down);
            if (!Physics.Raycast(ray, out RaycastHit hit, distanceToGround + 1f + maxDistance, groundLayer))
            {
                return new Tuple<Vector3, Quaternion>(_animator.GetIKPosition(goal), _animator.GetIKRotation(goal));
            }

            float footDistanceToAnimationPlane = _animator.GetIKPosition(goal).y - _animator.rootPosition.y ;

            Vector3 footPosition = hit.point;
            footPosition.y += footDistanceToAnimationPlane - animationToGround;
            
            _animator.SetIKPositionWeight(goal, _ikWeight);
            _animator.SetIKRotationWeight(goal, 1 - Mathf.Clamp01((footDistanceToAnimationPlane - (distanceToGround)) / (distanceToGround / 2.5f)));
            
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, hit.normal);
            return new Tuple<Vector3, Quaternion>(footPosition, Quaternion.LookRotation(forward, hit.normal));
        }

        private void SetFootTransform(AvatarIKGoal goal, Vector3 position, Quaternion rotation)
        {
            _animator.SetIKPosition(goal, position);
            _animator.SetIKRotation(goal, rotation);
        }
        
        private void LowerBody(Vector3 leftFoot, Vector3 rightFoot)
        {
            Vector3 lowestFoot = leftFoot.y < rightFoot.y ? leftFoot : rightFoot;
            var bodyPosition = _animator.bodyPosition;
            
            Vector3 direction = (bodyPosition - lowestFoot).normalized;
            float yProjection = Vector3.Dot(direction * legDistance, Vector3.up);
            
            bodyPosition = new Vector3(bodyPosition.x, Mathf.Lerp(bodyPosition.y, Mathf.Min(lowestFoot.y + yProjection, bodyPosition.y), _ikWeight), bodyPosition.z);
            
            _animator.bodyPosition = bodyPosition;
        }
    }
}