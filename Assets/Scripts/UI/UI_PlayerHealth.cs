using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// UI for player health. Manages displaying health bars.
    /// </summary>
    public class UI_PlayerHealth : MonoBehaviour
    {
        /// <summary>
        /// The health images to display.
        /// </summary>
        private List<Image> _healthImages;

        /// <summary>
        /// The prefab for the health image.
        /// </summary>
        [SerializeField] private GameObject healthImagePrefab;

        private void Awake()
        {
            _healthImages = GetComponentsInChildren<Image>(true).ToList();
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
            if (health > _healthImages.Count)
            {
                var healthToAdd = health - _healthImages.Count;
                for (var i = 0; i < healthToAdd; i++)
                {
                    var image = Instantiate(healthImagePrefab, transform);
                    _healthImages.Add(image.GetComponent<Image>());
                }
            }

            for (var i = 0; i < _healthImages.Count; i++)
            {
                _healthImages[i].enabled = i < health;
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