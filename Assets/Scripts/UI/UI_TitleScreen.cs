using UnityEngine;
using State;

namespace UI
{
    public class UI_TitleScreen : MonoBehaviour
    {
        /// <summary>
        /// UIPanel gameobject which contains the title screen
        /// </summary>
        [SerializeField] private GameObject ContentPanel;

        private void Start()
        {
            //Bind events
            UIManager.Instance.OnShowTitleScreen += EnablePanel;

        }

        private void OnDestroy()
        {
            //Unbind events
            UIManager.Instance.OnShowTitleScreen -= EnablePanel;

        }

        /// <summary>
        /// Enables the panel that holds the title screen
        /// </summary>
        /// <param name="state">Enabled?</param>
        public void EnablePanel(bool state)
        {
            if (ContentPanel == null) return;

            ContentPanel.SetActive(state);
        }

        /// <summary>
        /// On click handler for the "Start" button.
        /// Shows the title screen.
        /// </summary>
        public void OnClick_Start()
        {
            EnablePanel(false);

            //Start Game
            GameStateManager.Instance.OnGameStart?.Invoke();
        }

        /// <summary>
        /// On click handler for the "Settings" button.
        /// </summary>
        public void OnClick_Settings()
        {

        }

        /// <summary>
        /// On click handler for the "How To Play" button.
        /// </summary>
        public void OnClick_HowTo()
        {

        }
    }
}
