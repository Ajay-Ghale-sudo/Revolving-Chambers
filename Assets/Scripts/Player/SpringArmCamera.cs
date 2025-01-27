using System;
using DG.Tweening;
using Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Player
{
    /// <summary>
    /// Spring arm camera that follows a target object.
    /// </summary>
    public class SpringArmCamera : MonoBehaviour
    {
        /// <summary>
        /// Target object to follow.
        /// </summary>
        [Header("Camera Settings")]
        [Tooltip("Target object to follow")]
        [SerializeField] private Transform target;
        
        /// <summary>
        /// Enable zooming.
        /// </summary>
        [Tooltip("Enable zooming")]
        [SerializeField] private bool zoomEnabled = true;
        
        /// <summary>
        /// Distance from the target.
        /// </summary>
        [Tooltip("Distance from the target")]
        [SerializeField] private float currentArmLength = 10f;
        
        /// <summary>
        /// Minimum zoom distance.
        /// </summary>
        [Tooltip("Minimum zoom distance")]
        [SerializeField]
        private float minZoomDistance = 2f;
        
        /// <summary>
        /// Maximum zoom distance.
        /// </summary>
        [Tooltip("Maximum zoom distance")]
        [SerializeField]
        private float maxZoomDistance = 15f;
        
        /// <summary>
        /// Maximum pan distance.
        /// </summary>
        [Tooltip("Maximum pan distance")]
        [SerializeField]
        private float maxPanDistance = 12f;
        
        /// <summary>
        /// Pan speed of the camera.
        /// </summary>
        [Tooltip("Pan speed of the camera")]
        [SerializeField]
        private float panSpeed = 5f;
        
        /// <summary>
        /// Enable cursor tracking.
        /// </summary>
        [Tooltip("Enable cursor tracking")]
        [SerializeField]
        private bool trackCursor = true;
        
        /// <summary>
        /// Zoom speed of the camera.
        /// </summary>
        [Tooltip("Zoom speed of the camera")]
        [SerializeField] private float zoomSpeed = 10f;
        
        /// <summary>
        /// Smooth zoom interpolation speed.
        /// </summary>
        [Tooltip("Smooth zoom interpolation speed")]
        [SerializeField] private float smoothSpeed = 0.125f;
        
        /// <summary>
        /// Height offset from target.
        /// </summary>
        [Tooltip("Height offset from target")]
        [SerializeField] private float heightOffset = 2f;
        
        /// <summary>
        /// Camera pitch angle in degrees (0 = horizontal, 90 = straight down).
        /// </summary>
        [Tooltip("Camera pitch angle in degrees (0 = horizontal, 90 = straight down)")]
        [Range(0, 90)]
        [SerializeField] private float pitchAngle = 45f;
        
        /// <summary>
        /// Enable collision detection.
        /// </summary>
        [Header("Collision Settings")]
        [Tooltip("Enable camera collision")]
        [SerializeField] private bool enableCollision = true;
        
        /// <summary>
        /// Layers to check for camera collision.
        /// </summary>
        [Tooltip("Layers to check for camera collision")]
        [SerializeField] private LayerMask collisionLayers = -1;
        
        /// <summary>
        /// Collision radius of the camera.
        /// </summary>
        [Tooltip("Collision radius of the camera")]
        [SerializeField] private float collisionRadius = 0.2f;
        
        /// <summary>
        /// Camera shake duration.
        /// </summary>
        [Header("Camera Shake Settings")]
        [SerializeField] private float shakeDuration = 0.2f;
        
        /// <summary>
        /// Camera shake strength.
        /// </summary>
        [SerializeField] private float shakeStrength = 1f;
        
        /// <summary>
        /// Camera shake randomness.
        /// </summary>
        [SerializeField] private float shakeRandomness = 150f;
        
        /// <summary>
        /// Camera shake randomness mode.
        /// </summary>
        [SerializeField] private ShakeRandomnessMode shakeRandomnessMode = ShakeRandomnessMode.Harmonic;
        
        /// <summary>
        /// How much the screen will shake.
        /// </summary>
        [SerializeField] private int shakeVibrato = 10;
        
        /// <summary>
        /// Target position of the camera.
        /// </summary>
        private Vector3 targetPosition;
        
        /// <summary>
        /// Current position of the camera.
        /// </summary>
        private Vector3 currentPosition;
        
        /// <summary>
        /// Target arm length of the camera.
        /// </summary>
        private float targetArmLength;
        
        /// <summary>
        ///  Zoom input action.
        /// </summary>
        private InputAction zoomAction;
        
        /// <summary>
        ///  Camera zoom velocity.
        /// </summary>
        private float zoomVelocity = 0f;
        
        /// <summary>
        /// Tween for camera shake.
        /// </summary>
        private Tweener _shakeTween;
        
        /// <summary>
        /// The camera component.
        /// </summary>
        private Camera _camera;
        

        private void Start()
        {
           if (target == null)
           {
               Debug.LogError("Target object is not set on the SpringArmCamera component.");
               enabled = false;
           }
           
           zoomAction = InputSystem.actions.FindAction("Zoom");
           _camera = GetComponent<Camera>();
           
           currentPosition = transform.position;
           targetArmLength = currentArmLength;
           
           // Set initial rotation based on pitch angle
           transform.rotation = Quaternion.Euler(pitchAngle, 0, 0);
           
           BindCameraShake();
        }

        void BindCameraShake()
        {
            if (target.TryGetComponent(out Damageable damageable))
            {
                if (damageable.OnDamage == null)
                {
                    // Wait for the OnDamage event to be created
                    Invoke(nameof(BindCameraShake), 0.1f);
                    return;
                }
                damageable.OnDamage?.AddListener(CameraShake);
            }
            else
            {
                Debug.LogWarning("Target does not have IDamageable component.");
            }
        }

        private void Update()
        {
           // Handle Zoom input
           if (zoomEnabled && (zoomAction?.triggered ?? false))
           {
               onZoom(zoomAction.ReadValue<Vector2>().y);
           }
           
           // Smoothly interpolate the current arm length
           currentArmLength = Mathf.SmoothDamp(currentArmLength, targetArmLength, ref zoomVelocity, smoothSpeed);
        }

        private void OnDestroy()
        {
            if (target.TryGetComponent(out Damageable damageable))
            {
                damageable.OnDamage?.RemoveListener(CameraShake);
            }
        }

        /// <summary>
        /// Camera shake effect.
        /// </summary>
        private void CameraShake()
        {
            _shakeTween = _camera?.DOShakePosition(
                shakeDuration,
                shakeStrength,
                shakeVibrato,
                randomness: shakeRandomness,
                randomnessMode: shakeRandomnessMode)
                .SetId("CameraShake");
        }
        
        /// <summary>
        /// Handle zoom input.
        /// </summary>
        /// <param name="zoomAmount">Amount to zoom by.</param>
        private void onZoom(float zoomAmount)
        {
            targetArmLength = Mathf.Clamp(targetArmLength - zoomAmount * zoomSpeed, minZoomDistance, maxZoomDistance);
        }

        private void LateUpdate()
        {
            CalculatePosition();
        }

        /// <summary>
        /// Calculate the offset of the cursor. Used to determine the camera position between the target and the cursor.
        /// </summary>
        /// <returns>Middle point between cursor and target.</returns>
        private Vector3 CalculateCursorOffset()
        {
            if (!trackCursor) return target.position;
            
            // Get the cursor position in world space
            var cameraDistance = Mathf.Abs(transform.position.y - target.position.y);
            var mousePos = Mouse.current.position.ReadValue();
            var mouseVector = new Vector3(mousePos.x, mousePos.y, cameraDistance);
            var cursorPosition = _camera.ScreenToWorldPoint(mouseVector);
            cursorPosition.y = targetPosition.y;
            
            // Ensure the new point will still have the target in camera view
            var distance = Vector3.Distance(target.position, cursorPosition);
            if (distance > maxPanDistance)
            {
                cursorPosition = target.position + (cursorPosition - target.position).normalized * maxPanDistance;
            }
            
            // Calculate the middle point between the target and the cursor
            var middlePoint = (target.position + cursorPosition) / 2f;
            return middlePoint;
        }
        
        /// <summary>
        /// Calculate the next position of the camera.
        /// </summary>
        private void CalculatePosition()
        {
           // Calculate target position
           targetPosition = CalculateCursorOffset() + Vector3.up * heightOffset;
           
           // Calculate desired camera position based on fixed angle and current arm length
           var offset = Quaternion.Euler(pitchAngle, 0, 0) * Vector3.back * currentArmLength;
           var desiredPosition = targetPosition + offset;
           
           // Handle camera collision
           if (enableCollision) HandleCollision(ref desiredPosition);
           currentPosition = desiredPosition;

           // No zooming while camera shaking for now
           if (_shakeTween?.IsActive() ?? false) return;
           // Get difference between Current and Target position and smooth motion
           var difference = currentPosition - transform.position;
           transform.position += difference * (panSpeed * Time.deltaTime);
           transform.rotation = Quaternion.LookRotation(targetPosition - currentPosition, Vector3.up);
        }

        /// <summary>
        /// Checks the camera for collisions and adjusts the desired position accordingly.
        /// </summary>
        /// <param name="desiredPosition">The cameras desired position.</param>
        private void HandleCollision(ref Vector3 desiredPosition)
        {
            var direction = (desiredPosition - targetPosition).normalized;
            var distance = Vector3.Distance(targetPosition, desiredPosition);
            if (Physics.SphereCast(targetPosition, collisionRadius, direction, out var hit, distance, collisionLayers))
            {
                desiredPosition = targetPosition + direction * (hit.distance);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!enabled || target == null) return;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(target.position + Vector3.up * heightOffset, 0.1f);
            Gizmos.DrawLine(target.position + Vector3.up * heightOffset, transform.position);

            if (enableCollision)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, collisionRadius);
            }
        }
    }
}