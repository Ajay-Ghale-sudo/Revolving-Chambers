using System;
using UnityEngine;

namespace Events
{
    /// <summary>
    /// Data for the <see cref="AudioEvent"/>.
    /// </summary>
    [Serializable]
    public class AudioEventData : GameEventData
    {
        /// <summary>
        /// Audio clip to play.
        /// </summary>
        [Tooltip("Audio clip source")]
        [SerializeField]
        public AudioClip clip;
        
        /// <summary>
        /// Volume of the audio clip.
        /// </summary>
        [Tooltip("Playback volume")]
        [Range(0.0f, 1.0f)]
        [SerializeField]
        public float volume = 1;
        
        /// <summary>
        /// Pitch of the audio clip.
        /// </summary>
        [Tooltip("Playback pitch")]
        [Range(0.0f, 12.0f)]
        [SerializeField]
        public float pitch = 1;
        
        /// <summary>
        /// Whether the audio clip should loop.
        /// </summary>
        [Tooltip("Loop playback")]
        [SerializeField]
        public bool loop = false;
    }
    
    /// <summary>
    /// Event that plays audio.
    /// </summary>
    [CreateAssetMenu(fileName = "Data", menuName = "Game Events/Audio", order = 0)]
    public class AudioEvent : GameEvent<AudioEventData>
    {
        /// <summary>
        /// Event that is triggered when the <see cref="AudioEvent"/> is invoked.
        /// </summary>
        public static Action<AudioEventData> OnAudioEvent;
        
        protected override void OnInvoke(GameObject invoker = null)
        {
            base.OnInvoke(invoker);
            OnAudioEvent?.Invoke(data);
        }
    }
}