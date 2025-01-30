using System;
using DG.Tweening;
using Events;
using State;
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
        /// An object used to control background music. 
        /// </summary>
        private BackgroundMusicController _backgroundMusicController;
        
        /// <summary>
        /// The <see cref="AudioListener"/> attached to this GameObject. Used when there is no player audio listener.
        /// </summary>
        private AudioListener _audioListener;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            
            GameObject musicObject = new GameObject("BackgroundMusicController");
            musicObject.transform.SetParent(transform);
            _backgroundMusicController = musicObject.AddComponent<BackgroundMusicController>();

            AudioEvent.OnAudioEvent += PlaySound;
            LocationAudioEvent.OnLocationAudioEvent += PlaySoundAtPosition;
            _audioListener ??= gameObject.AddComponent<AudioListener>();
            _audioListener.enabled = false;
            
            GameStateManager.Instance.OnGameOver += OnGameOver;
            GameStateManager.Instance.OnGameStart += ResetGlobalAudioPitch;
            GameStateManager.Instance.OnPlayerRevive += ResetGlobalAudioPitch;
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
            
            GameStateManager.Instance.OnGameOver -= OnGameOver;
            GameStateManager.Instance.OnGameStart -= ResetGlobalAudioPitch;
            GameStateManager.Instance.OnPlayerRevive -= ResetGlobalAudioPitch;
        }

        /// <summary>
        /// Sets the ambient clip and volume.
        /// Used for setting new clips.
        /// </summary>
        /// <remarks> Will stop playing if audioClip is null </remarks>
        /// <param name="ambientClip">Sound to play</param>
        /// <param name="volume">Volume from 0.0 - 1.0</param>
        public void SetBackgroundMusic(AudioClip audioClip, float volume)
        {
            if (!audioClip)
            {
                PlayBackgroundMusic(false);
                return;
            }

            ClearBackgroundMusicEffects();
            _backgroundMusicController?.SetClip(audioClip, volume);
            _backgroundMusicController?.Play();
        }

        /// <summary>
        /// Method for Start/Stop ambient AudioSource.
        /// Used for pausing and resuming.
        /// </summary>
        /// <param name="state">start or stop</param>
        public void PlayBackgroundMusic(bool state)
        {
            if (_backgroundMusicController ==null) return;

            if (state)
            {
                _backgroundMusicController?.Play();
            }
            else
            {
                _backgroundMusicController?.Stop();
            }
        }

        /// <summary>
        /// Automates a volume envelope transition on the background music.
        /// </summary>
        /// <param name="startVolume">Start volume level</param>
        /// <param name="targetVolume">Target volume level</param>
        /// <param name="tweenTime">Length of the envelope in seconds</param>
        /// <param name="easeType">Type of easing to use for the envelope</param>
        public void StartBackgroundMusicVolumeEnvelope(float startVolume, float targetVolume, float tweenTime,
            Ease easeType = Ease.Linear)
        {
            _backgroundMusicController?.ApplyVolumeEnvelope(startVolume, targetVolume, tweenTime, easeType);
        }

        /// <summary>
        /// Sets the cutoff frequency for the background music's lowpass filter.
        /// </summary>
        /// <param name="targetFreq">Cutoff frequency in Hz</param>
        public void SetBackgroundMusicLowpassFilter(float targetFreq)
        {
            _backgroundMusicController?.UpdateLowpassFilter(targetFreq);
        }

        /// <summary>
        /// Interpolates the pitch of the background music between two pitches over a specified timeframe
        /// </summary>
        /// <param name="startPitch">Starting pitch</param>
        /// <param name="targetPitch">Target pitch</param>
        /// <param name="tweenTime">Time to take in seconds</param>
        /// <param name="easeType">Type of tween easing to use</param>
        public void StartBackgroundMusicPitchEnvelope(float startPitch, float targetPitch, float tweenTime,
            Ease easeType = Ease.Linear)
        {
            _backgroundMusicController?.ApplyPitchEnvelope(startPitch, targetPitch, tweenTime, easeType);
        }

        /// <summary>
        /// Interpolates a highpass filter on the BGM between two cutoff frequencies over a specified timeframe.
        /// </summary>
        /// <param name="startFreq">Start HPF cutoff frequency in Hz</param>
        /// <param name="targetFreq">Target HPF cutoff frequency in Hz</param>
        /// <param name="tweenTime">Time to take in seconds</param>
        /// <param name="easeType">Type of tween easing to use</param>
        public void StartBackgroundMusicHighpassFilterEnvelope(float startFreq, float targetFreq, float tweenTime,
            Ease easeType = Ease.Linear)
        {
            _backgroundMusicController?.ApplyHighpassFilterEnvelope(startFreq, targetFreq, tweenTime, easeType);
        }

        /// <summary>
        /// Interpolates a lowpass filter on the BGM between two cutoff frequencies over a specified timeframe.
        /// </summary>
        /// <param name="startFreq">Start LPF cutoff frequency in Hz</param>
        /// <param name="targetFreq">Final LPF cutoff frequency in Hz</param>
        /// <param name="tweenTime">Time to take in seconds</param>
        /// <param name="easeType">Type of tween easing to use</param>
        public void StartBackgroundMusicLowpassFilterEnvelope(float startFreq, float targetFreq, float tweenTime,
            Ease easeType = Ease.Linear)
        {
            _backgroundMusicController?.ApplyLowpassFilterEnvelope(startFreq, targetFreq, tweenTime, easeType);
        }

        /// <summary>
        /// Interpolates a reverb level on the BGM between two reverb levels over a specified timeframe.
        /// </summary>
        /// <param name="startLevel">Initial volume level on [0.0, 1.0]</param>
        /// <param name="targetLevel">Target volume level on [0.0, 1.0]</param>
        /// <param name="tweenTime">Envelope length in seconds</param>
        /// <param name="easeType">Type of tween easing to use</param>
        public void StartBackgroundMusicReverbLevelEnvelope(float startLevel, float targetLevel, float tweenTime,
            Ease easeType = Ease.Linear)
        {
            _backgroundMusicController?.ApplyReverbEnvelope(startLevel, targetLevel, tweenTime, easeType);
        }

        public void ClearBackgroundMusicEffects()
        {
            _backgroundMusicController?.ClearEffects();
        }

        /// <summary>
        /// Play the specified sound.
        /// </summary>
        /// <param name="audioData">The sound to play</param>
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
        
        /// <summary>
        /// Adjust audio pitch
        /// </summary>
        /// <param name="rate"></param>
        public void AdjustGlobalPitch(float rate)
        {
            _audioSource.pitch = rate;
            _backgroundMusicController.SetPitch(rate);
        }
        
        /// <summary>
        /// Reset audio playrate
        /// </summary>
        public void ResetGlobalAudioPitch()
        {
            _audioSource.pitch = 1f;
            _backgroundMusicController?.SetPitch(1.0f);
        }
        
        /// <summary>
        /// Pause all audio.
        /// </summary>
        public void PauseAudio()
        {
            _audioSource?.Pause();
            _backgroundMusicController?.Pause();
        }

        /// <summary>
        /// Handle game over.
        /// </summary>
        private void OnGameOver()
        {
            ResetGlobalAudioPitch();
            PauseAudio();
            // TODO: Eventually play game over sound.
        }
    }
}