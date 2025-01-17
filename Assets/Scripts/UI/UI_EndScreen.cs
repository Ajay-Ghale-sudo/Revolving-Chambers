using UnityEngine;
using TMPro;

namespace UI
{
    public class UI_EndScreen : MonoBehaviour
    {
        /// <summary>
        /// UIPanel gameobject which contains the end screen
        /// </summary>
        [SerializeField] private GameObject ContentPanel;

        private void Start()
        {
            //Bind events
            UIManager.Instance.OnShowEndScreen += EnablePanel;
        }

        private void OnDestroy()
        {
            //Unbind events
            UIManager.Instance.OnShowEndScreen -= EnablePanel;
        }

        /// <summary>
        /// Enables the panel that holds the end screen
        /// </summary>
        /// <param name="state">Enabled?</param>
        public void EnablePanel(bool state)
        {
            if (ContentPanel == null) return;

            ContentPanel.SetActive(state);
        }

        /// <summary>
        /// On click handler for the "Okay" button.
        /// Shows the title screen.
        /// </summary>
        public void OnClick_Okay()
        {
            EnablePanel(false);
            UIManager.Instance.OnShowTitleScreen?.Invoke(true);
        }
    }
}
