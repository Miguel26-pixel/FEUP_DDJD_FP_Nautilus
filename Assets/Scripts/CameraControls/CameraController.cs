using PlayerControls;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CameraControls
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public Vector3 cameraOffset = new Vector3(1.5f, 4.6f, -8f);
        public float cameraHorizontalRotationFactor = 0.3f;
        public float cameraVerticalRotationFactor = 0.1f;
        public float cameraRotationInterpolation = 0.1f;
        public float maxVerticalAngle = 70f;
        public float minVerticalAngle = -70f;
        public LayerMask layerMask;
        
        private Camera _camera;
        private Transform _cameraTransform;
        private Transform _cameraParentTransform;
        private Player _player;
        
        private Vector2 _remainingAngle;
        private float _currentVerticalAngle;
        private bool _isLocked;

        private void Start()
        {
            _camera = GetComponent<Camera>();
            _cameraTransform = _camera.transform;
            _player = GameObject.FindWithTag("Player").GetComponent<Player>();
            
            _cameraTransform.position = _player.transform.position + cameraOffset;
            _cameraParentTransform = _cameraTransform.parent;
        }

        private void LateUpdate()
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            if (_isLocked)
            {
                mouseDelta = Vector2.zero;
            }
            _remainingAngle += DeltaToDegrees(mouseDelta);

            if ((_remainingAngle.y + _currentVerticalAngle) > maxVerticalAngle)
            {
                _remainingAngle.y = maxVerticalAngle - _currentVerticalAngle;
            }
            else if ((_remainingAngle.y + _currentVerticalAngle) < minVerticalAngle)
            {
                _remainingAngle.y = minVerticalAngle - _currentVerticalAngle;
            }

            float angle = _remainingAngle.x * cameraRotationInterpolation;
            _remainingAngle.x -= angle;
            _remainingAngle.x = _remainingAngle.x is < 0.01f and > -0.01f ? 0f : _remainingAngle.x;
            var position = _player.transform.position;
            _cameraParentTransform.RotateAround(position, Vector3.up, angle);

            float yAngle = _remainingAngle.y * cameraRotationInterpolation;
            _remainingAngle.y -= yAngle;
            _currentVerticalAngle += yAngle;
            _remainingAngle.y = _remainingAngle.y is < 0.01f and > -0.01f ? 0f : _remainingAngle.y;
            _cameraTransform.RotateAround(position + _cameraTransform.rotation * cameraOffset, _cameraTransform.right, yAngle);
            
            _cameraTransform.position = position + _cameraTransform.rotation * cameraOffset;
            
            // Check for collision
            if (Physics.Linecast(position + new Vector3(cameraOffset.x, cameraOffset.y, 0), _cameraTransform.position, out var hit, layerMask))
            {
                _cameraTransform.position = hit.point + hit.normal * 0.1f;
            }
        }

        public Transform GetHorizontalTransform()
        {
            return _cameraParentTransform;
        }
        
        private Vector2 DeltaToDegrees(Vector2 delta)
        {
            return new Vector2(delta.x * cameraHorizontalRotationFactor, -delta.y * cameraVerticalRotationFactor);
        }
        
        public void Lock()
        {
            _isLocked = true;
        }
        
        public void Unlock()
        {
            _isLocked = false;
        }
    }
}
