using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// MonoBehaviour component for controlling the UI wheel
    /// </summary>
    public class UI_WheelController : MonoBehaviour
    {
        /// <summary>
        /// Wheel fill prefab to instantiate when a new section is added
        /// </summary>
        public GameObject WheelFillPrefab;

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

        /// <summary>
        /// Cache all fill gameobjects
        /// </summary>
        private List<GameObject> _allFills;

        private void Awake()
        {
            _transform = GetComponent<RectTransform>();
            Notch_Max = 1;
            Notch_Current = 0;
            _allFills = new List<GameObject>();
        }

        private void Start()
        {
            UpdateWheel(360, 0);
            
            AddSection(0, 90, Color.white);
            AddSection(90, 180, Color.red);
            AddSection(180, 270, Color.green);
            AddSection(270, 360, Color.blue);
        }

        /// <summary>
        /// Updates the wheel notches and sections
        /// </summary>
        /// <param name="notchMax">Sets the maximum number of notches on the wheel</param>
        public void UpdateWheel(int notchMax, int notchCurrent = 0)
        {
            Notch_Max = notchMax == 0 ? 1 : notchMax; //Don't set it to 0

            UpdateRotation(notchCurrent);
        }

        /// <summary>
        /// Updates the wheel rotation to a notch. Rotates clockwise
        /// </summary>
        /// <param name="notch"></param>
        public void UpdateRotation(int notch)
        {
            //Calculate degrees based on notches
            Notch_Current = notch % Notch_Max;
            float deg = Notch_Current / (Notch_Max / 360.0f);

            _transform.localRotation = Quaternion.Euler(0, 0, deg);
        }

        /// <summary>
        /// Adds a new section to the wheel
        /// </summary>
        /// <param name="min">the starting notch</param>
        /// <param name="max">the last notch</param>
        /// <param name="color">display data</param>
        public void AddSection(int min, int max, Color color)
        {
            Image newSection = InstantiateFill().GetComponent<Image>();
            if (newSection == null) return;

            newSection.color = color;

            //Calculate fill amount
            float fillAmt = (max - min) / (Notch_Max * 1.0f); //Convert to float in the calculation by multiplying by 1.0f
            newSection.fillAmount = fillAmt;

            //Calculate fill rotation (starting rotation)
            float rotAmt = min / (Notch_Max / 360.0f);
            newSection.transform.localRotation = Quaternion.Euler(0, 0, -rotAmt);
        }

        /// <summary>
        /// Instantiate new fill gameobject
        /// </summary>
        private GameObject InstantiateFill()
        {
            if (WheelFillPrefab == null) return null;

            GameObject newSection = Instantiate(WheelFillPrefab, _transform);
            _allFills.Add(newSection);

            return newSection;
        }

        /// <summary>
        /// Destroys all sections
        /// </summary>
        private void ClearSections()
        {
            foreach (GameObject section in _allFills)
            {
                Destroy(section);
            }

            _allFills.Clear();
        }
    }
}
