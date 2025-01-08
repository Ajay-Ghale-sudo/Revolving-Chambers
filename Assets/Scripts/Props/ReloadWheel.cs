using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Weapon;

namespace Props
{
    
    /// <summary>
    /// Reload wheel for selecting ammo.
    /// </summary>
    public class ReloadWheel : MonoBehaviour
    {

        /// <summary>
        /// Section of the wheel.
        /// </summary>
        [Serializable]
        public class WheelSection
        {
            /// <summary>
            /// Rewards for this section.
            /// </summary>
            public string name;
            
            /// <summary>
            /// Probability of this section.
            /// </summary>
            public float probability;
            
            /// <summary>
            /// Color of the section.
            /// </summary>
            // TODO: Should this come from the ammo object?
            public Color sectionColor;
            
            /// <summary>
            /// Ammo for this section.
            /// </summary>
            public Ammo ammo;

            /// <summary>
            /// Start angle of the section.
            /// </summary>
            internal float Start;
            
            /// <summary>
            /// End angle of the section.
            /// </summary>
            internal float End;
        }

        /// <summary>
        /// Sections of the wheel.
        /// </summary>
        [SerializeField]
        private List<WheelSection> wheelSections = new();
        
        /// <summary>
        /// Duration of the spin.
        /// </summary>
        [SerializeField]
        private float spinDuration = 1f;
        
        /// <summary>
        /// Animation curve for the spin
        /// </summary>
        [SerializeField]
        private AnimationCurve spinCurve;
        
        /// <summary>
        /// Whether the wheel is spinning.
        /// </summary>
        private bool _isSpinning = false;
        
        /// <summary>
        /// Current value of the wheel.
        /// </summary>
        private float _currentValue = 0f;
        
        /// <summary>
        /// Selected section of the wheel.
        /// </summary>
        private WheelSection _selectedSection;
        
        /// <summary>
        /// Tween for spinning the wheel.
        /// </summary>
        private Tweener _spinTween;

        /// <summary>
        /// Event invoked when the wheel stops spinning.
        /// </summary>
        public UnityEvent<WheelSection> OnWheelStop;
        
        /// <summary>
        /// Event invoked when a section is created.
        /// </summary>
        public Action<WheelSection> OnSectionCreated;

        private void Start()
        {
            var totalProbability = 0f;
            foreach (var wheelSection in wheelSections)
            {
                totalProbability += wheelSection.probability;
            }
            
            if (Mathf.Abs(totalProbability - 1f) > 0.01f)
            {
                Debug.LogError("Wheel section probabilities do not add up to 1.");
            }
            
            ReloadManager.Instance.OnSpinStart += SpinWheel;
            ReloadManager.Instance.OnSpinEnd += StopWheel;
            
            CreateWheelSections();
        }

        /// <summary>
        /// Create the wheel sections.
        /// </summary>
        void CreateWheelSections()
        {
            var currentAngle = 0f;
            foreach (var section in wheelSections)
            {
                var sectionAngle = (section.probability * 360f); // Review, prob don't need to conform to a circle here
                section.Start = currentAngle;
                currentAngle += sectionAngle;
                section.End = currentAngle;
                
                OnSectionCreated?.Invoke(section);
                ReloadManager.Instance.OnSpinSectionAdded?.Invoke((int)section.Start, (int)section.End, section.sectionColor);
            }
        }

        /// <summary>
        /// Start the wheel spinning tween.
        /// </summary>
        private void SpinWheel()
        {
            if (_isSpinning) return;

            _isSpinning = true;
            _spinTween = DOVirtual.Float(0f, 360f, spinDuration, WheelUpdate)
                .SetEase(spinCurve)
                .OnKill(HandleWheelStop);
        }

        
        /// <summary>
        /// Update tick of the wheel spin.
        /// </summary>
        /// <param name="value">Current notch value of the wheel</param>
        private void WheelUpdate(float value)
        {
            Debug.Log($"WheelUpdate: {value}");
            _currentValue = value;
            ReloadManager.Instance.OnSpinUpdate?.Invoke((int)_currentValue);
        }

        /// <summary>
        /// Handle the wheel stopping.
        /// </summary>
        private void HandleWheelStop()
        {
            _isSpinning = false;
            
            // Calculate the selected section
            _selectedSection =  wheelSections.First(section => _currentValue >= section.Start && _currentValue <= section.End);
            
            OnWheelStop?.Invoke(_selectedSection);
            ReloadManager.Instance.OnLoadAmmo?.Invoke(_selectedSection.ammo);
        }

        /// <summary>
        /// Stop the wheel spinning tween.
        /// </summary>
        private void StopWheel()
        {
            if (!_isSpinning) return;
            _spinTween?.Kill();
        }
    }
}