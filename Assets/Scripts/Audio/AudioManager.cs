using System;
using Events;
using UnityEngine;
using Utility;

namespace Audio
{
    /// <summary>
    /// Manager for audio.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : Singleton<AudioManager> 
    {
        /// <summary>
        /// The <see cref="AudioSource"/> attached to this GameObject.
        /// </summary>
        private AudioSource _audioSource;

        /// <summary>
        /// A separate AudioSource for playing ambient audio. Allows for independent volume/settings control 
        /// </summary>
        private AudioSource _ambientAudioSource;
        
        /// <summary>
        /// The <see cref="AudioListener"/> attached to this GameObject. Used when there is no player audio listener.
        /// </summary>
        private AudioListener _audioListener;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _ambientAudioSource = gameObject.AddComponent<AudioSource>();
            _ambientAudioSource.loop = true;

            AudioEvent.OnAudioEvent += PlaySound;
            LocationAudioEvent.OnLocationAudioEvent += PlaySoundAtPosition;
            _audioListener ??= gameObject.AddComponent<AudioListener>();
            _audioListener.enabled = false;
        }
        
        /// <summary>
        /// Enable the default <see cref="_audioListener"/>
        /// </summary>
        /// <param name="state">The state to set the AudioListener</param>
        public void EnableDefaultListener(bool state)
        {
            if (!_audioListener) return;
            _audioListener.enabled = state;
        }

        private void Start()
        {
            _audioListener.enabled = false;
        }

        private void OnDestroy()
        {
            AudioEvent.OnAudioEvent -= PlaySound;
            LocationAudioEvent.OnLocationAudioEvent -= PlaySoundAtPosition;
        }

        /// <summary>
        /// Sets the ambient clip and volume.
        /// Used for setting new clips.
        /// </summary>
        /// <remarks> Will stop playing if audioClip is null </remarks>
        /// <param name="ambientClip">Sound to play</param>
        /// <param name="volume">Volume from 0.0 - 1.0</param>
        public void SetAmbientClip(AudioClip audioClip, float volume)
        {
            if (!audioClip)
            {
                PlayAmbient(false);
                return;
            }

            _ambientAudioSource.clip = audioClip;
            _ambientAudioSource.volume = volume;
            PlayAmbient(true);
        }

        /// <summary>
        /// Method for Start/Stop ambient AudioSource.
        /// Used for pausing and resuming.
        /// </summary>
        /// <param name="state">start or stop</param>
        public void PlayAmbient(bool state)
        {
            if (_ambientAudioSource == null) return;

            if (state && _ambientAudioSource.clip)
            {
                _ambientAudioSource?.Play();
            }
            else
            {
                _ambientAudioSource?.Stop();
            }
        }

        /// <summary>
        /// Play the specified sound.
        /// </summary>
        /// <param name="audioData">The sound to play.</param>
        public void PlaySound(AudioEventData audioData)
        {
            if (!audioData?.clip) return;

            // Play the sound (don't crash).
            _audioSource.pitch = audioData.pitch;
            _audioSource?.PlayOneShot(audioData.clip, audioData.volume);
        }
        
        /// <summary>
        /// Play the specified sound at the specified position.
        /// </summary>
        /// <param name="audioData">The audio data to play</param>
        public void PlaySoundAtPosition(LocationAudioEventData audioData)
        {
            if (!audioData?.clip) return;

            // Play the sound at the specified position.
            AudioSource.PlayClipAtPoint(audioData.clip, audioData.location, audioData.volume); 
        }
    }
}