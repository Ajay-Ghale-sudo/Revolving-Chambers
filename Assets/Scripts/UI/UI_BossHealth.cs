using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class UI_BossHealth : MonoBehaviour
    {
        /// <summary>
        /// TMPro gameobject for displaying boss name
        /// </summary>
        [Tooltip("TMPro gameobject for displaying boss name")]
        [SerializeField] private TextMeshProUGUI BossNameText;

        /// <summary>
        /// Image for displaying boss health.
        /// </summary>
        [Tooltip("Image for displaying boss health.")]
        [SerializeField] private Image HealthBarImage;
        
        /// <summary>
        /// Image for displaying boss health showing damage done.
        /// </summary>
        [Tooltip("Image for displaying boss health showing damge done.")]
        [SerializeField] private Image DamageHealthBarImage;
        

        /// <summary>
        /// Animation curve for the fill amount.
        /// </summary>
        [Tooltip("Animation curve for the fill amount.")]
        [SerializeField] private AnimationCurve _fillCurve;
        
        /// <summary>
        /// Health bar fill duration.
        /// </summary>
        [Tooltip("How long it takes the damage health bar to change.")]
        [SerializeField] private float _fillDuration = 1f;
        
        /// <summary>
        /// Health bar shake duration.
        /// </summary>
        [Header("UI Shake Settings")]
        [Tooltip("Shake duration")]
        [SerializeField] private float shakeDuration = 0.5f;
        
        /// <summary>
        /// Health bar shake strength.
        /// </summary>
        [Tooltip("Shake strength")]
        [SerializeField] private float shakeStrength = 10f;
        
        /// <summary>
        /// Health bar shake vibrato.
        /// </summary>
        [Tooltip("Shake vibrato")]
        [SerializeField] private int shakeVibrato = 10;

        /// <summary>
        /// Starting fill amount of the damage health bar.
        /// </summary>
        private float _startingFillAmount;
        
        /// <summary>
        /// Target fill amount of the damage health bar.
        /// </summary>
        private float _targetFillAmount;
        
        /// <summary>
        /// Current duration of the fill amount.
        /// </summary>
        private float _currentDuration = 0f;
        

        private void Awake()
        {
            BindUIEvents();
            _startingFillAmount = DamageHealthBarImage.fillAmount;
            _targetFillAmount = DamageHealthBarImage.fillAmount;
        }

        private void Start()
        {
        }

        private void OnDestroy()
        {
            UnbindUIEvents();
        }

        private void Update()
        {
            UpdateDamageBar();
        }

        /// <summary>
        /// Update the damage bar fill amount.
        /// </summary>
        private void UpdateDamageBar()
        {
            // This may not work if the boss is healed.
            if (_currentDuration > _fillDuration) return;
            
            _currentDuration += Time.deltaTime;
            var t = _fillCurve?.Evaluate(_currentDuration / _fillDuration) ?? _currentDuration / _fillDuration;
            DamageHealthBarImage.fillAmount = Mathf.Lerp(_startingFillAmount, _targetFillAmount, t);

        }

        /// <summary>
        /// Bind UI events.
        /// </summary>
        private void BindUIEvents()
        {
            UIManager.Instance.OnBossHealthChange += SetHealth;
            UIManager.Instance.OnBossSpawned += SetName;
        }

        /// <summary>
        /// Unbind UI events.
        /// </summary>
        private void UnbindUIEvents()
        {
            UIManager.Instance.OnBossHealthChange -= SetHealth;
            UIManager.Instance.OnBossSpawned -= SetName;
        }

        /// <summary>
        /// Changes the name display
        /// </summary>
        /// <param name="newName">New text</param>
        public void SetName(string newName)
        {
            if (BossNameText == null) return;

            BossNameText.text = newName;
        }

        /// <summary>
        /// Changes the fill amount of the health bar.
        /// Uses Image's horizontal fill setting.
        /// </summary>
        /// <param name="fillAmount">Size of bar in 0.0f - 1.0f</param>
        public void SetHealth(float fillAmount)
        {
            if (!HealthBarImage) return;

            if (fillAmount < HealthBarImage.fillAmount)
            {
                Damaged();
            }
            
            HealthBarImage.fillAmount = fillAmount;
            _startingFillAmount = DamageHealthBarImage.fillAmount;
            _targetFillAmount = fillAmount;
            _currentDuration = 0f;
        }

        /// <summary>
        /// Shake the health bar when damaged.
        /// </summary>
        private void Damaged()
        {
           HealthBarImage.transform.parent.DOShakePosition(
               shakeDuration,
               shakeStrength,
               shakeVibrato,
               fadeOut: true); 
        }
    }
}
