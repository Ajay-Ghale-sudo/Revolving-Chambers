using System;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Player health bar represented as a poker chip
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class UI_HealthChip : MonoBehaviour
    {
        /// <summary>
        /// The force to launch the chip with
        /// </summary>
        [SerializeField] private float launchForce = 5f;
        
        /// <summary>
        /// The torque to apply to the chip when launched
        /// </summary>
        [SerializeField] private float launchTorque = 20f;
        
        /// <summary>
        /// The starting position of the chip
        /// </summary>
        private Vector3 _startPosition;
        
        /// <summary>
        /// The starting rotation of the chip
        /// </summary>
        private Vector3 _startRotation;
        
        
        /// <summary>
        /// The <see cref="Rigidbody"/> attached to this GameObject.
        /// </summary>
        private Rigidbody _rigidbody;
        
        /// <summary>
        /// Whether the chip is disabled
        /// </summary>
        bool _disabled = false;
        
        private void Awake()
        {
            _startPosition = transform.position;
            _startRotation = transform.eulerAngles;
            _rigidbody = GetComponent<Rigidbody>();
            
            _rigidbody.isKinematic = true;
        }
        
        /// <summary>
        /// Reset the position of the chip
        /// </summary>
        public void ResetPosition()
        {
            _disabled = false;
            _rigidbody.isKinematic = true;
            transform.position = _startPosition;
            transform.eulerAngles = _startRotation;
        }

        /// <summary>
        /// Launch the chip in the air and then disable it
        /// </summary>
        public void Launch()
        {
            _rigidbody.isKinematic = false;
            _rigidbody.AddForce(Vector3.up * launchForce, ForceMode.Impulse);
            _rigidbody.AddTorque(Vector3.left * launchTorque, ForceMode.Impulse);
            
            _disabled = true;
            Invoke(nameof(UpdateActive), 2f);
        }

        public void OnEnable()
        {
           ResetPosition(); 
        }

        /// <summary>
        /// Update the active state of the chip
        /// </summary>
        public void UpdateActive()
        {
            gameObject.SetActive(!_disabled);
        }
    }
}