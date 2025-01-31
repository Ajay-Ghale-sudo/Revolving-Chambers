using UnityEngine;
using Events;
using State;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace UI
{

    public class UI_PausePanel : MonoBehaviour
    {
        /// <summary>
        /// Panel which contains all the content
        /// </summary>
        [SerializeField] private GameObject _contentPanel;

        /// <summary>
        /// The event for loading the hub scene
        /// </summary>
        [SerializeField] private LoadLevelEvent _loadHubSceneEvent;

        /// <summary>
        /// The gameobject with a button component for the "Abandon" action
        /// </summary>
        [SerializeField] private GameObject _abandonButton;

        /// <summary>
        /// The slider for volume control
        /// </summary>
        [SerializeField] private Slider _volumeSlider;

        /// <summary>
        /// All chip images (poker chip next to each button)
        /// </summary>
        [SerializeField] private List<Image> _chipList;

        /// <summary>
        /// Is the panel enabled
        /// </summary>
        private bool _enabled = false;

        private void Start()
        {
            //Bind events
            UIManager.Instance.OnTogglePauseScreen += TogglePanel;
        }

        private void OnDestroy()
        {
            //Unbind events
            UIManager.Instance.OnTogglePauseScreen -= TogglePanel;
        }

        /// <summary>
        /// Enables/disables the panel like a toggle
        /// </summary>
        public void TogglePanel()
        {
            ShowPanel(!_enabled);
        }

        /// <summary>
        /// Enables/disables the panel that holds the title screen
        /// </summary>
        public void ShowPanel(bool state)
        {
            if (_contentPanel == null) return;

            _enabled = state;
            _contentPanel.SetActive(state);

            GameStateManager.Instance?.OnGamePause.Invoke(state);

            //Updatevolume slider and "Abandon" button
            if (state)
            {
                //Update slider to the current value
                if (_volumeSlider != null)
                {
                    _volumeSlider.value = AudioListener.volume;
                }

                //Only show "Abandon" button if we are not in the Hub scene
                if (_abandonButton != null)
                {
                    _abandonButton.SetActive(SceneManager.GetActiveScene().name != "HUBScene");
                }
            }
            else //Disable all poker chip icons
            {
                foreach (Image img in _chipList)
                {
                    img.enabled = false;
                }
            }
        }

        /// <summary>
        /// On click handler for the "Resume" button.
        /// </summary>
        public void OnClick_Resume()
        {
            ShowPanel(false);
        }

        /// <summary>
        /// On click handler for the "How to Play" button.
        /// </summary>
        public void OnClick_HowTo()
        {
            //TODO: Settings page
        }

        /// <summary>
        /// On click handler for the "Abandon" button.
        /// </summary>
        public void OnClick_Abandon()
        {
            ShowPanel(false);
            _loadHubSceneEvent?.Invoke();
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
