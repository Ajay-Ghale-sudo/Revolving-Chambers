using System;
using UnityEngine;

namespace UI
{
    public class UI_TutorialPanel : MonoBehaviour
    {
        private void Start()
        {
           UIManager.Instance.OnTogglePauseScreen += HandlePause; 
        }

        private void OnDestroy()
        {
            UIManager.Instance.OnTogglePauseScreen -= HandlePause;
        }

        private void HandlePause()
        {
            gameObject.SetActive(false); 
        }
    }
}