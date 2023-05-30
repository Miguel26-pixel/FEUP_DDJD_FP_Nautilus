using PlayerControls;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CameraControls
{
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public Vector3 cameraOffset = new Vector3(1.5f, 4.6f, -8f);
        public float cameraRotationFactor = 1f;
        public float cameraRotationInterpolation = 0.1f;
        
        private Camera _camera;
        private Transform _cameraTransform;
        private Player _player;
        
        private Vector2 _remainingAngle;

        private void Start()
        {
            _camera = GetComponent<Camera>();
            _cameraTransform = _camera.transform;
            _player = GameObject.FindWithTag("Player").GetComponent<Player>();
            
            _cameraTransform.position = _player.transform.position + cameraOffset;
        }

        private void LateUpdate()
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            _remainingAngle += new Vector2(DeltaToDegrees(mouseDelta.x), DeltaToDegrees(mouseDelta.y));
            
            float angle = _remainingAngle.x * cameraRotationInterpolation;
            _remainingAngle.x -= angle;
            _remainingAngle.x = _remainingAngle.x is < 0.01f and > -0.01f ? 0f : _remainingAngle.x;
            var position = _player.transform.position;
            _cameraTransform.RotateAround(position, Vector3.up, angle);

            // if (mouseDelta.y > 0)
            // {
            //     
            // }

            _cameraTransform.position = position + _cameraTransform.rotation * cameraOffset;
        }
        
        private float DeltaToDegrees(float delta)
        {
            return delta * cameraRotationFactor;
        }
    }
}
