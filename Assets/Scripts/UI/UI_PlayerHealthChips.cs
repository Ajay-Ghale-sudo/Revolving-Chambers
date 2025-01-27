using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// UI for player health. Manages displaying health bars.
    /// </summary>
    public class UI_PlayerHealthChips : MonoBehaviour
    {
        /// <summary>
        /// The health segments to display.
        /// </summary>
        private List<UI_HealthChip> _healthSegments;

        /// <summary>
        /// The prefab for the health object.
        /// </summary>
        [SerializeField] private GameObject healthPrefab;

        private void Awake()
        {
            _healthSegments = GetComponentsInChildren<UI_HealthChip>().ToList();
        }

        private void Start()
        {
            UpdateUI();
            BindUIEvents();
        }

        private void OnDestroy()
        {
            UnbindUIEvents();
        }

        /// <summary>
        /// Bind UI events.
        /// </summary>
        private void BindUIEvents()
        {
            UIManager.Instance.OnPlayerDamage += UpdateUI;
            UIManager.Instance.OnPlayerDeath += UpdateUI;
            UIManager.Instance.OnPlayerHealthChange += UpdateHealthBars;
        }

        /// <summary>
        /// Unbind UI events.
        /// </summary>
        private void UnbindUIEvents()
        {
            UIManager.Instance.OnPlayerDamage -= UpdateUI;
            UIManager.Instance.OnPlayerDeath -= UpdateUI;
            UIManager.Instance.OnPlayerHealthChange -= UpdateHealthBars;
        }

        /// <summary>
        /// Update the health bars.
        /// </summary>
        /// <param name="health">Current amount of health.</param>
        private void UpdateHealthBars(int health)
        {
            if (health > _healthSegments.Count)
            {
                var healthToAdd = health - _healthSegments.Count;
                for (var i = 0; i < healthToAdd; i++)
                {
                    var image = Instantiate(healthPrefab, transform);
                    _healthSegments.Add(image.GetComponent<UI_HealthChip>());
                }
            }

            for (var i = 0; i < _healthSegments.Count; i++)
            {
                var healthSegment = _healthSegments[i];
                var shouldBeActive = i < health;
                if (health > 0 && !shouldBeActive && healthSegment.gameObject.activeSelf)
                {
                    healthSegment.Launch();
                }
                else
                {
                    _healthSegments[i].gameObject.SetActive(shouldBeActive);
                }
            }
        }

        /// <summary>
        ///  Refresh the UI.
        /// </summary>
        void UpdateUI()
        {
        }
    }
}