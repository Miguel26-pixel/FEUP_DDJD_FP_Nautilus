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
        
        private Camera _camera;
        private Transform _cameraTransform;
        private Transform _cameraParentTransform;
        private Player _player;
        
        private Vector2 _remainingAngle;
        private float _currentVerticalAngle;

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
            _remainingAngle += DeltaToDegrees(mouseDelta);

            _remainingAngle.y = (_remainingAngle.y + _currentVerticalAngle) switch
            {
                > 90f => 90f - _currentVerticalAngle,
                < -90f => -90f - _currentVerticalAngle,
                _ => _remainingAngle.y
            };

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
        }
        
        public Transform GetHorizontalTransform()
        {
            return _cameraParentTransform;
        }
        
        private Vector2 DeltaToDegrees(Vector2 delta)
        {
            return new Vector2(delta.x * cameraHorizontalRotationFactor, -delta.y * cameraVerticalRotationFactor);
        }
    }
}
