using Events;
using UnityEngine;
using State;
using UnityEngine.UI;

namespace UI
{
    public class UI_TitleScreen : MonoBehaviour
    {
        /// <summary>
        /// UIPanel gameobject which contains the title screen
        /// </summary>
        [SerializeField] private GameObject ContentPanel;

        /// <summary>
        /// Event to play audio associated with a button click.
        /// </summary>
        [SerializeField] private AudioEvent ButtonClickEvent;
        
        /// <summary>
        /// Event to play audio associated with a button hover.
        /// </summary>
        [SerializeField] private AudioEvent ButtonHoverEvent;

        /// <summary>
        /// The slider for volume control
        /// </summary>
        [SerializeField] private Slider _volumeSlider;

        private void Start()
        {
            //Bind events
            UIManager.Instance.OnShowTitleScreen += EnablePanel;

            if(_volumeSlider != null)
            {
                _volumeSlider.value = AudioListener.volume;
            }
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
            ButtonClickEvent?.Invoke();
            
            EnablePanel(false);

            //Start Game
            GameStateManager.Instance.OnGameStart?.Invoke();
        }

        /// <summary>
        /// On click handler for the "Settings" button.
        /// </summary>
        public void OnClick_Settings()
        {
            ButtonClickEvent?.Invoke();
        }

        /// <summary>
        /// On click handler for the "How To Play" button.
        /// </summary>
        public void OnClick_HowTo()
        {
            ButtonClickEvent?.Invoke();
        }

        /// <summary>
        /// Handler for hovering over a button in the UI.
        /// </summary>
        public void OnHover_Enter()
        {
            ButtonHoverEvent?.Invoke();
        }

        /// <summary>
        /// Handler for mouse leaving a button in the UI.
        /// </summary>
        public void OnHover_Exit()
        {
        }

        /// <summary>
        /// Called by slider's OnValueChanged
        /// </summary>
        /// <param name="value">Slider's value</param>
        public void OnChange_Volume(float value)
        {
            GameStateManager.Instance?.OnVolumeChange.Invoke(value);
        }
    }
}
