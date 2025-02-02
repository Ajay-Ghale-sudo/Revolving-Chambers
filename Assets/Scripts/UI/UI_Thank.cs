using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UI_Thank : MonoBehaviour
    {
        
        [SerializeField] private float _fadeInDuration = 2.5f;
        private void Start()
        {
            Invoke(nameof(FadeIn), _fadeInDuration);
        }


        private void FadeIn()
        {
            Debug.Log("Fading in");
             // enable and fade in children
             var children = GetComponentsInChildren<Transform>(true);
             foreach (var child in children)
             {
                 var childObject = child.gameObject;
                 childObject.SetActive(true);
                 
                 if (child.TryGetComponent<RawImage>(out var rawImage))
                 {
                     rawImage.CrossFadeAlpha(0f, 0f, true);
                     rawImage.CrossFadeAlpha(1f, _fadeInDuration, true);
                 }
                 else if (child.TryGetComponent<TextMeshProUGUI>(out var text))
                 {
                     text.CrossFadeAlpha(0f, 0f, true);
                     text.CrossFadeAlpha(1f, _fadeInDuration, true);
                 }
             }
        }
    }
}