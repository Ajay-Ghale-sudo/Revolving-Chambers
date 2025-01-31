using Audio;
using UnityEngine;

namespace Props
{
    public class LevelIntroProxy : MonoBehaviour
    {
        /// <summary>
        /// The audio clip to play and loop as background music after the intro.
        /// </summary>
        [SerializeField]
        private AudioClip _backgroundMusicClip;
        
        private void Awake()
        {
            AudioManager.Instance?.SetBackgroundMusic(_backgroundMusicClip, 0.6f);
        }
    }
}