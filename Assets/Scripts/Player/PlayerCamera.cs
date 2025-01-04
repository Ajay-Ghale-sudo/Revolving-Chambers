using System;
using System.Collections;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// Camera that follows the player.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class PlayerCamera : MonoBehaviour
    {
        /// <summary>
        /// The target to follow.
        /// </summary>
        [SerializeField] private Transform Target;

        /// <summary>
        /// Viewport boundary that the player can move within.
        /// </summary>
        [SerializeField] private Rect _viewBox;

        /// <summary>
        /// When to start following the player (% of the screen)
        /// </summary>
        [Header("Bounds Settings")] [Range(0f, 0.5f)]
        [SerializeField]
        private float _boundaryThreshold = 0.2f;

        /// <summary>
        /// The camera component.
        /// </summary>
        private Camera _camera;

        /// <summary>
        /// Coroutine to move the camera.
        /// </summary>
        private Coroutine _moveCameraCoroutine;

        /// <summary>
        /// The target camera position.
        /// </summary>
        private Vector3 _targetCameraPosition;
        
        /// <summary>
        /// The camera velocity.
        /// </summary>
        private Vector3 _cameraVelocity = Vector3.zero;

        /// <summary>
        /// The camera speed.
        /// </summary>
        [SerializeField] private float _cameraSpeed = 3.5f;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _targetCameraPosition = transform.position;
            
            // lock cursor
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }

        private void Start()
        {
            InitViewRect();
            _moveCameraCoroutine = StartCoroutine(MoveCamera());
            
            // Set the camera to look at the player
            transform.LookAt(Target);
        }

        private void Update()
        {
            MoveCamera();
        }

        private void LateUpdate()
        {
            // AdjustCamera();
        }

        /// <summary>
        /// Initializes the view rect viewport boundary.
        /// </summary>
        private void InitViewRect()
        {
            _viewBox = new Rect(_boundaryThreshold, _boundaryThreshold, 1 - _boundaryThreshold, 1 - _boundaryThreshold);
        }

        /// <summary>
        /// Moves the camera to follow the player.
        /// </summary>
        /// <returns></returns>
        private IEnumerator MoveCamera()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                CalculateTargetPosition();
            }
        }

        /// <summary>
        /// Moves the camera toward the target position.
        /// </summary>
        private void AdjustCamera()
        {
            var direction = _targetCameraPosition - _camera.transform.position;
            direction.y = 0;
            if (!(direction.magnitude > 0.1f) && !(direction.magnitude < .5f)) return;
            
            // Speed up camera movement if player is further away
            var speed = direction.magnitude > .2f ? _cameraSpeed * 3.5f * direction.magnitude : _cameraSpeed;
            var newPos = _camera.transform.position + direction.normalized * (Time.deltaTime * speed);
            _camera.transform.position = newPos;
        }

        /// <summary>
        /// Calculates the target position for the camera.
        /// </summary>
        private void CalculateTargetPosition()
        {
            var playerPos = Target.transform.position;
            var cameraPos = _camera.transform.position;
            var cursorPos = Input.mousePosition;
            var centerScreen = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            var cursorWorldPos = _camera.ScreenToWorldPoint(new Vector3(cursorPos.x, cursorPos.y, 0));
            var playerScreenPos = _camera.WorldToViewportPoint(playerPos);
            
            var cursorNormalX = cursorPos.x / Screen.width;
            var cursorNormalY = cursorPos.y / Screen.height;

            cameraPos.x = cursorNormalX;
            cameraPos.z = cursorNormalY;
            
            // TODO: This is incomplete, needs to offset the camera based on player and cursor positions.
            
            if (cameraPos != _camera.transform.position) _targetCameraPosition = cameraPos;
        }

        /// <summary>
        /// Debug drawing
        /// </summary>
        private void OnDrawGizmos()
        {
            if (_camera == null) return;

            Gizmos.color = Color.red;
            var cameraPosition = transform.position;

            // Draw deadzone rect
            var topLeft = _camera.ViewportToWorldPoint(new Vector3(_viewBox.x, _viewBox.y, cameraPosition.z));
            var topRight =
                _camera.ViewportToWorldPoint(new Vector3(_viewBox.x + _viewBox.width, _viewBox.y, cameraPosition.z));
            var bottomLeft =
                _camera.ViewportToWorldPoint(new Vector3(_viewBox.x, _viewBox.y + _viewBox.height, cameraPosition.z));
            var bottomRight = _camera.ViewportToWorldPoint(new Vector3(_viewBox.x + _viewBox.width,
                _viewBox.y + _viewBox.height, cameraPosition.z));

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);

            Gizmos.DrawCube(_targetCameraPosition, Vector3.one * 3);
        }
    }
}