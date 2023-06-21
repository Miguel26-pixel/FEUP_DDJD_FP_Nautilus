using System;
using System.Collections;
using Crafting;
using DataManager;
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
        [Range(-1f, 2f)] 
        public float distanceToGround = 0.1f;
        public float animationToGround = 0.1f;

        public float legDistance = 33.76f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask waterLayer;

        private float _speed;
        private float _targetSpeed;
        private bool _isRunning;

        private Vector2 _currentMovementInput;
        private Vector3 _currentMovement;
        private bool _isMovementPressed;
        private bool _movementLocked;

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

        [Header("Attacking")]
        public float throwForce;
        public float throwUpwardForce;
        public float throwCooldown;
        private Weapon currentWeapon = null;
        public GameObject leftHand;
        public GameObject rightHand;
        int i = 0;

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

        LineRenderer lr;
        Rigidbody rb;
        Vector3 startPosition;
        Vector3 startVelocity;
        float InitialAngle = -20;
        Quaternion rot;

        [Header("Swim Event")]
        public float waterDistance = 1;

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
        
            lr=GetComponent<LineRenderer>();
            rot = Quaternion.Euler(InitialAngle,0,0);

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
                _targetSpeed = _isRunning ? runningSpeed : walkingSpeed;
            }
            else
            {
                _targetSpeed = 0.0f;
            }

            _speed = Math.Abs(_speed - _targetSpeed) > 0.01f
                ? Mathf.Lerp(_speed, _targetSpeed, Time.deltaTime * 10.0f)
                : _targetSpeed;


            HandleAttack();
            _cameraRelativeMovement = ConvertToCameraSpace(_currentMovement);
            HandleWater();
            HandleRotation();
            HandleAnimation();
            _characterController.Move(_cameraRelativeMovement * (Time.deltaTime * _speed) + _jumpVelocity * Time.deltaTime);
            HandleGravity();
            HandleJump();
        }

        public void HandleAttack()
        {

            if (_movementLocked || currentWeapon==null)
            {
                return;
            }

            Rigidbody projectileRb = currentWeapon.GetComponent<Rigidbody>();


            if(Input.GetMouseButtonDown(0) && _readyToThrow)
            {
                drawline(projectileRb);
            }
            if(Input.GetMouseButtonUp(0) && _readyToThrow)
            {
                _animator.SetTrigger("Attack");
            }
        }

        public void HandleThrow()
        {
            Debug.Break();
            Rigidbody projectileRb = currentWeapon.GetComponent<Rigidbody>();
            currentWeapon.gameObject.transform.parent = null;
            projectileRb.isKinematic = false;
            projectileRb.useGravity = true;

            projectileRb.velocity += rot*(throwUpwardForce*transform.forward);

            lr.enabled=false;

            currentWeapon = null;
            Invoke(nameof(ResetThrow), throwCooldown);

        }

        private void drawline(Rigidbody projectileRb)
        {
            i = 0;
            lr.positionCount = 2000;
            lr.enabled = true;
            startPosition=currentWeapon.transform.position;
            startVelocity=rot*(throwUpwardForce*currentWeapon.transform.forward)/projectileRb.mass;
            lr.SetPosition(i,startPosition);
            for (float j=0; i<lr.positionCount-1;j+=0.5f)
            {
                i++;
                Vector3 linePosition=startPosition+j*startVelocity;
                linePosition.y=startPosition.y+startVelocity.y*j+ 0.5f * Physics.gravity.y * j * j;
                lr.SetPosition(i,linePosition);
            }
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

        public void OnPickup(InputAction.CallbackContext context)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 3.0f);

            Weapon weapon = null;
            Collider c = null;
            float nearestDistance = float.MaxValue;

            foreach (Collider collider in colliders)
            {
                Debug.Log(collider);
                Weapon w = collider.GetComponent<Weapon>();
                if (w != null)
                {
                    Debug.Log(w);
                    float distance = Vector3.Distance(transform.position, w.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        weapon = w;
                        c = collider;
                    }
                }
            }

            Debug.Log(weapon);
            Debug.Log(c);

            if(weapon != null && c != null)
            {
                Rigidbody weaponRigidBody = weapon.GetComponent<Rigidbody>();
                LaserGunWeapon laser = c.GetComponent<LaserGunWeapon>();
                SpearWeapon spear = c.GetComponent<SpearWeapon>();
                GameObject hand = null;

                if(laser != null)
                    hand = rightHand;
                else if(spear != null)
                    hand = leftHand;

                Debug.Log(hand);

                if (weapon != null)
                {
                    if (currentWeapon != null)
                        currentWeapon.GetComponent<Weapon>().DeactivateWeapon();

                    currentWeapon = weapon;
                    weapon.gameObject.transform.parent = hand.transform;
                    weapon.transform.localPosition = weapon.PickPosition;
                    weaponRigidBody.isKinematic = true;
                    weapon.transform.localEulerAngles  = weapon.PickRotation;
                    _animator.SetFloat("CurrentWeapon", weapon.weaponId);
                    weapon.ActivateWeapon();
                }

            }
        }

        private void HandleWater()
        {
            if (Physics.Raycast(transform.position, Vector3.up, out var hit, 10f, waterLayer))
            {
                float distance = Mathf.Clamp01((transform.position.y - hit.point.y)/waterDistance);
                 _animator.SetFloat("WaterDistance", distance);
            }
            else {
                _animator.SetFloat("WaterDistance", 0);

            }
        }

        private void HandleAnimation()
        {
            float animSpeed;
            if (_speed <= walkingSpeed)
            {
                animSpeed = _speed / walkingSpeed;
            }
            else
            {
                animSpeed = (_speed - walkingSpeed) / (runningSpeed - walkingSpeed) + 1;
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
            if (_characterController.isGrounded)
            {
                _jumpVelocity.y = GroundGravity;
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

        // private void Throw()
        // {
        //     if (_movementLocked || currentWeapon==null)
        //     {
        //         return;
        //     }
            
        //     _readyToThrow = false;

        //     Rigidbody projectileRb = currentWeapon.GetComponent<Rigidbody>();

        //     Vector3 forceToAdd = _cameraTransform.transform.forward * throwForce + transform.up * throwUpwardForce;
        
        //     currentWeapon.gameObject.transform.parent = null;
        //     projectileRb.isKinematic = false;
        //     projectileRb.useGravity = true;

        //     _animator.SetTrigger("Attack");

        //     float Vi = Mathf.Sqrt(10 * -Physics.gravity.y / (Mathf.Sin(Mathf.Deg2Rad * 90 * 2)));
        //     float Vy, Vz;   

        //     Vz = Vi * Mathf.Cos(Mathf.Deg2Rad * 90);

        //     Vector3 localVelocity = new Vector3(0f, 0f, Vz);
            
        //     Vector3 globalVelocity = transform.TransformVector(localVelocity);

        //     projectileRb.velocity += _cameraTransform.transform.forward * Time.deltaTime;
        //     projectileRb.velocity *= Mathf.Clamp01(1f - throwForce * Time.deltaTime);

        //     currentWeapon = null;

        //     Invoke(nameof(ResetThrow), throwCooldown);

        // 

        // private void OnCollisionEnter(Collision other)
        // {
        //     if (other.collider.CompareTag("Weapon"))
        //     {
        //         Weapon weapon = other.collider.GetComponent<Weapon>();
        //         Rigidbody weaponRigidBody = weapon.GetComponent<Rigidbody>();
        //         LaserGunWeapon laser = other.collider.GetComponent<LaserGunWeapon>();
        //         SpearWeapon spear = other.collider.GetComponent<SpearWeapon>();
        //         GameObject hand = null;

        //         if(laser != null)
        //             hand = rightHand;
        //         else if(spear != null)
        //             hand = leftHand;

        //         Debug.Log(hand);

        //         if (weapon != null)
        //         {
        //             if (currentWeapon != null)
        //                 currentWeapon.GetComponent<Weapon>().DeactivateWeapon();

        //             currentWeapon = weapon;
        //             weapon.gameObject.transform.parent = hand.transform;
        //             weapon.transform.localPosition = weapon.PickPosition;
        //             weaponRigidBody.isKinematic = true;
        //             weapon.transform.localEulerAngles  = weapon.PickRotation;
        //             _animator.SetFloat("CurrentWeapon", weapon.weaponId);
        //             weapon.ActivateWeapon();
        //         }
        //     }
        // }

        private void ResetThrow()
        {
            _readyToThrow = true;
        }

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