using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using Weapon;

namespace Wheel
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
        /// Color of the section. Typically derived from the payload.
        /// </summary>
        public virtual Color SectionColor => Color.clear;
        
        /// <summary>
        /// Start angle of the section.
        /// </summary>
        internal float Start;

        /// <summary>
        /// End angle of the section.
        /// </summary>
        internal float End;
    }

    public abstract class RewardWheel<T> : MonoBehaviour where T : WheelSection
    {
        /// <summary>
        /// Active wheel sections.
        /// </summary>
        [SerializeField] protected List<T> wheelSections = new();
        
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
        protected T _selectedSection;
        
        /// <summary>
        /// Tween for spinning the wheel.
        /// </summary>
        private Tweener _spinTween;

        /// <summary>
        /// Event invoked when the wheel stops spinning.
        /// </summary>
        public UnityEvent<T> OnWheelStop;
        
        /// <summary>
        /// Event invoked when a section is created.
        /// </summary>
        public Action<T> OnSectionCreated;

        private void Start()
        {
            SetupWheel();
        }

        protected virtual void SetupWheel()
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
            
            CreateWheelSections();
        }
        
        /// <summary>
        /// Create the wheel sections.
        /// </summary>
        protected void CreateWheelSections()
        {
            ReloadManager.Instance.OnClearSections?.Invoke();
            var currentAngle = 0f;
            foreach (var section in wheelSections)
            {
                var sectionAngle = (section.probability * 360f); // Review, prob don't need to conform to a circle here
                section.Start = currentAngle;
                currentAngle += sectionAngle;
                section.End = currentAngle;
                
                OnSectionCreated?.Invoke(section);
                ReloadManager.Instance.OnSpinSectionAdded?.Invoke((int)section.Start, (int)section.End, section.SectionColor);
            }
        }
        
        /// <summary>
        /// Start the wheel spinning tween.
        /// </summary>
        protected void SpinWheel()
        {
            if (_isSpinning) return;

            _isSpinning = true;
            _spinTween = DOVirtual.Float(0f, 360f, spinDuration, WheelUpdate)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(spinCurve)
                .SetUpdate(true)
                .OnKill(HandleWheelStop);
        }

        /// <summary>
        /// Stop the wheel spinning tween.
        /// </summary>
        protected void StopWheel()
        {
            if (!_isSpinning) return;
            _spinTween?.Kill();
        }
        
        /// <summary>
        /// Update tick of the wheel spin.
        /// </summary>
        /// <param name="value">Current notch value of the wheel</param>
        protected void WheelUpdate(float value)
        {
            _currentValue = value;
            ReloadManager.Instance.OnSpinUpdate?.Invoke((int)_currentValue);
        }

        /// <summary>
        /// Handle the wheel stopping.
        /// </summary>
        private void HandleWheelStop()
        {
            _isSpinning = false;
            SelectSection();
        }

        /// <summary>
        /// Select the section of the wheel at the current value.
        /// </summary>
        private void SelectSection()
        {
            _selectedSection =  wheelSections.First(section => _currentValue >= section.Start && _currentValue <= section.End);
            OnWheelStop?.Invoke(_selectedSection);
            SectionSelected(_selectedSection);
        }
        
        protected virtual void SectionSelected(T section)
        {
        }
    }
}