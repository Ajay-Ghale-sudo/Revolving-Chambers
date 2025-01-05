using UnityEngine;
using System.Collections;

namespace UI
{
    /// <summary>
    /// MonoBehaviour component for controlling the UI wheel
    /// </summary>
    public class UI_WheelController : MonoBehaviour
    {
        /// <summary>
        /// Max number of 'notches' on the wheel
        /// </summary>
        private int Notch_Max;

        /// <summary>
        /// Current notch on the wheel
        /// </summary>
        private int Notch_Current;

        /// <summary>
        /// Cache this gameobject's RectTransform
        /// </summary>
        private RectTransform _transform;

        private void Awake()
        {
            _transform = GetComponent<RectTransform>();
            Notch_Max = 1;
            Notch_Current = 0;
        }

        private void Start()
        {
            UpdateWheel(360, 0);
        }

        /// <summary>
        /// Updates the wheel notches and sections
        /// TODO: wheel sections
        /// </summary>
        /// <param name="notchMax">Sets the maximum number of notches on the wheel</param>
        public void UpdateWheel(int notchMax, int notchCurrent = 0)
        {
            Notch_Max = notchMax == 0 ? 1 : notchMax; //Don't set it to 0

            UpdateRotation(notchCurrent);
        }

        /// <summary>
        /// Updates the wheel rotation to a notch
        /// </summary>
        /// <param name="notch"></param>
        public void UpdateRotation(int notch)
        {
            //Calculate degrees based on notches
            Notch_Current = notch % Notch_Max;
            float deg = Notch_Current / (Notch_Max / 360.0f);

            _transform.localRotation = Quaternion.Euler(0, 0, -deg);
        }
    }
}
