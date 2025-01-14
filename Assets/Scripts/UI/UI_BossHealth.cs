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

        private void Start()
        {
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
        /// <param name="name">New text</param>
        public void SetName(string name)
        {
            if (BossNameText == null) return;

            BossNameText.text = name;
        }

        /// <summary>
        /// Changes the fill amount of the health bar.
        /// Uses Image's horizontal fill setting.
        /// </summary>
        /// <param name="fillAmount">Size of bar in 0.0f - 1.0f</param>
        public void SetHealth(float fillAmount)
        {
            if (HealthBarImage == null) return;

            HealthBarImage.fillAmount = fillAmount;
        }
    }
}
