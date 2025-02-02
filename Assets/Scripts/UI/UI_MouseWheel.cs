using UnityEngine;
using System.Collections;
using State;
using UnityEngine.UI;
using Weapon;

namespace UI
{
    public class UI_MouseWheel : MonoBehaviour
    {
        /// <summary>
        /// RectTransform of the panel the cursor is on
        /// </summary>
        [SerializeField] private RectTransform _cursorCanvas;

        /// <summary>
        /// The panel that contains the cursor
        /// </summary>
        [SerializeField] private GameObject _contentPanel;

        /// <summary>
        /// The panel that contains the reload wheel
        /// </summary>
        [SerializeField] private GameObject _reloadWheel;

        /// <summary>
        /// The Image component of the crosshair
        /// </summary>
        [SerializeField] private Image _cursorTexture;

        /// <summary>
        /// Sprite for the loaded crosshair
        /// </summary>
        [SerializeField] private Sprite _loadedCrosshair;

        /// <summary>
        /// Sprite for the empty crosshair
        /// </summary>
        [SerializeField] private Sprite _emptyCrosshair;

        /// <summary>
        /// Is this component currently paused
        /// </summary>
        private bool _paused = false;

        private void Start()
        {
            GameStateManager.Instance.OnGamePause += OnPause;
            GameStateManager.Instance.OnPlayerRevive += OnSpinEnd;
            GameStateManager.Instance.OnGameOver += OnGameOver;

            UIManager.Instance.OnPointerMove += OnPoint;
            UIManager.Instance.OnNextChamber += OnNextChamber;
            UIManager.Instance.OnDeathWheelStart += OnSpinStart;

            ReloadManager.Instance.OnSpinEnd += OnSpinEnd;
            ReloadManager.Instance.OnSpinStart += OnSpinStart;
            ReloadManager.Instance.OnLoadAmmo += OnLoadAmmo;
        }

        private void OnDestroy()
        {
            GameStateManager.Instance.OnGamePause -= OnPause;
            GameStateManager.Instance.OnPlayerRevive -= OnSpinEnd;
            GameStateManager.Instance.OnGameOver -= OnGameOver;

            UIManager.Instance.OnPointerMove -= OnPoint;
            UIManager.Instance.OnNextChamber -= OnNextChamber;
            UIManager.Instance.OnDeathWheelStart -= OnSpinStart;

            ReloadManager.Instance.OnSpinEnd -= OnSpinEnd;
            ReloadManager.Instance.OnSpinStart -= OnSpinStart;
            ReloadManager.Instance.OnLoadAmmo -= OnLoadAmmo;
        }

        /// <summary>
        /// Enables/Disables the custom cursor, including the wheel.
        /// </summary>
        /// <param name="state"></param>
        public void EnableCursor(bool state)
        {
            if (_contentPanel == null) return;

            if (state)
            {
                _contentPanel.SetActive(true);
            }
            else
            {
                _contentPanel.SetActive(false);
            }
        }

        /// <summary>
        /// Enables/Disables the spinning wheel around the cursor.
        /// Cursor must be enabled also to see the wheel.
        /// </summary>
        /// <param name="state"></param>
        public void EnableWheel(bool state)
        {
            if (_reloadWheel == null) return;

            if (state)
            { 
                _reloadWheel.SetActive(true);
            }
            else
            {
                _reloadWheel.SetActive(false);
            }
        }

        /// <summary>
        /// Sets the custom cursor to the loaded sprite
        /// </summary>
        public void SetLoadedCursor()
        {
            if (_cursorTexture == null || _loadedCrosshair == null) return;

            _cursorTexture.sprite = _loadedCrosshair;
        }

        /// <summary>
        /// Sets the custom cursor to the empty sprite
        /// </summary>
        public void SetEmptyCursor()
        {
            if (_cursorTexture == null || _emptyCrosshair == null) return;

            _cursorTexture.sprite = _emptyCrosshair;
        }

        /// <summary>
        /// Sets the colout of the image
        /// </summary>
        public void SetCursorColor(Color color)
        {
            if (_cursorTexture == null) return;

            _cursorTexture.color = color;
        }

        /// <summary>
        /// Moves the reload wheel to the cursor position.
        /// Called by the Player Input system when cursor moves. 
        /// Player Input system -> CharacterMovement -> UIManager -> OnPoint
        /// </summary>
        /// <param name="inputData">screen space position of mouse in Vector2</param>
        public void OnPoint(Vector2 inputData)
        {
            if (_paused || _contentPanel == null) return;

            Vector2 rectPos = Vector2.zero;

            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(_cursorCanvas, inputData, null, out rectPos))
            {
                RectTransform rectTrans = _contentPanel.transform as RectTransform;
                rectTrans.anchoredPosition = rectPos;
            }
        }

        /// <summary>
        /// Called via GameStateManager.OnGamePause.
        /// Sets cursor visibility and pauses custom cursor activity.
        /// </summary>
        /// <param name="state">Pause state</param>
        public void OnPause(bool state)
        {
            _paused = state;

            //Disable custom cursor and use system cursor when paused
            EnableCursor(!state);
            Cursor.visible = state;
        }

        /// <summary>
        /// Hides the spin wheel
        /// </summary>
        public void OnSpinEnd()
        {
            //no delay right now because the wheel sections change right after reloading
            EnableWheel(false);
        }

        /// <summary>
        /// Enables the spin wheel
        /// </summary>
        public void OnSpinStart()
        {
            EnableWheel(true);
        }

        /// <summary>
        /// Disables the spin wheel and enables cursor
        /// </summary>
        public void OnGameOver()
        {
            EnableCursor(false);
            Cursor.visible = true;
        }

        /// <summary>
        /// Sets cursor to loaded.
        /// Changes colour to ammo
        /// </summary>
        /// <param name="ammo">Ammo loaded</param>
        public void OnLoadAmmo(Ammo ammo)
        {
            SetLoadedCursor();
            SetCursorColor(ammo.color);
        }

        /// <summary>
        /// Chamber switched on revolver.
        /// Set the cursor to empty if no ammo
        /// </summary>
        /// <param name="chamber"></param>
        public void OnNextChamber(Revolver.RevolverChamber chamber)
        {
            if (chamber.IsEmpty)
            {
                SetEmptyCursor();
                SetCursorColor(Color.black);
            }
            else
            {
                SetLoadedCursor();
                SetCursorColor(chamber.Ammo.color);
            }
        }
    }
}
