using System;
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

namespace Audio
{
    public class BackgroundMusicController : MonoBehaviour
    {
        private AudioSource musicSource;
        private AudioHighPassFilter highpassFilter;
        private AudioLowPassFilter lowpassFilter;
        private AudioReverbFilter reverbFilter;
        
        private const string BGMTweenID = "BackgroundMusicTween";

        private const float DefaultMusicVolume = 1.0f;
        private const float DefaultMusicPitch = 1.0f;
        private const float DefaultHighpassCutoffFreq = 20.0f;
        private const float DefaultLowpassCutoffFreq = 20.0f;
        private const float DefaultReverbLevel = -10000.0f;

        private void Awake()
        {
            if (!TryGetComponent(out musicSource))
            {
                musicSource = gameObject.AddComponent<AudioSource>();
            }
            musicSource.loop = true;
            musicSource.volume = DefaultMusicVolume;
            musicSource.pitch = DefaultMusicPitch;
            
            if (!TryGetComponent(out highpassFilter))
            {
                highpassFilter = gameObject.AddComponent<AudioHighPassFilter>();
                // low enough to not affect the sound by default
                highpassFilter.cutoffFrequency = DefaultHighpassCutoffFreq;
            }
            
            if (!TryGetComponent(out lowpassFilter))
            {
                lowpassFilter = gameObject.AddComponent<AudioLowPassFilter>();
                // high enough to not affect the sound by default
                lowpassFilter.cutoffFrequency = DefaultLowpassCutoffFreq;
            }

            if (!TryGetComponent(out reverbFilter))
            {
                reverbFilter = gameObject.AddComponent<AudioReverbFilter>();
                // set to minimum, since we don't want to hear reverb by default
                reverbFilter.reverbLevel = DefaultReverbLevel;
            }
        }

        private void OnDestroy()
        {
            DOTween.Kill(BGMTweenID);
        }

        /// <summary>
        /// Gets the AudioClip currently assigned as background music if there is one.
        /// </summary>
        /// <returns></returns>
        public AudioClip GetClip()
        {
            return musicSource?.clip;
        }

        /// <summary>
        /// Sets a new AudioClip as background music.
        /// </summary>
        /// <param name="clip">The clip to assign as background music</param>
        /// <param name="volume">The volume of the new background music clip</param>
        /// <param name="pitch">The pitch of the new background music clip</param>
        public void SetClip(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
        {
            if (musicSource == null) return;
            musicSource.clip = clip;
            musicSource.volume = volume;
            musicSource.pitch = pitch;
        }

        /// <summary>
        /// Gets the volume of the current background music.
        /// </summary>
        /// <returns>The volume of the current background music</returns>
        public float GetVolume()
        {
            if (musicSource == null) return 0;
            return musicSource.volume;
        }

        /// <summary>
        /// Sets the volume of the current background music.
        /// </summary>
        /// <param name="volume"></param>
        public void SetVolume(float volume)
        {
            if (musicSource == null) return;
            musicSource.volume = volume;
        }

        /// <summary>
        /// Gets the pitch of the current background music.
        /// </summary>
        /// <returns>The pitch of the current background music</returns>
        public float GetPitch()
        {
            if (musicSource == null) return 0;
            return musicSource.pitch;
        }

        /// <summary>
        /// Sets the pitch of the current background music.
        /// </summary>
        /// <param name="pitch">New pitch to assign to the background music</param>
        public void SetPitch(float pitch)
        {
            if (musicSource == null) return;
            musicSource.pitch = pitch;
        }

        /// <summary>
        /// Begins playing the current background music if an AudioClip is assigned.
        /// </summary>
        public void Play()
        {
            if (musicSource == null || musicSource.clip == null) return;
            musicSource.Play();
        }

        /// <summary>
        /// Stops playing the current background music.
        /// </summary>
        public void Stop()
        {
            // Even if the clip has somehow been unset, we want to be able to stop playing music
            if (musicSource == null) return;
            musicSource.Stop();
        }

        /// <summary>
        /// Clears the state of all effects and all effects tweens on the background music.
        /// </summary>
        public void ClearEffects()
        {
            DOTween.Kill(BGMTweenID);
            if (musicSource == null) return;

            musicSource.volume = DefaultMusicVolume;
            musicSource.pitch = DefaultMusicPitch;
            
            if (highpassFilter != null) highpassFilter.cutoffFrequency = DefaultHighpassCutoffFreq;
            if (lowpassFilter != null) lowpassFilter.cutoffFrequency = DefaultLowpassCutoffFreq;
            if (reverbFilter != null) reverbFilter.reverbLevel = DefaultReverbLevel;
        }

        /// <summary>
        /// Updates the highpass filter's cutoff frequency instantly.
        /// </summary>
        /// <param name="cutoff">The cutoff frequency in Hz for the highpass filter</param>
        public void UpdateHighpassFilter(float cutoff)
        {
            if (highpassFilter == null) return;
            highpassFilter.cutoffFrequency = cutoff;
        }

        /// <summary>
        /// Updates the lowpass filter instantly.
        /// </summary>
        /// <param name="cutoff">The cutoff frequency in Hz for the lowpass filter</param>
        public void UpdateLowpassFilter(float cutoff)
        {
            if (lowpassFilter == null) return;
            lowpassFilter.cutoffFrequency = cutoff;
        }

        /// <summary>
        /// Applies a specified volume envelope to the current background music.
        /// </summary>
        /// <param name="startVolume">The initial volume value</param>
        /// <param name="endVolume">The final volume value</param>
        /// <param name="tweenDuration">The duration of the envelope in seconds</param>
        /// <param name="easeType">The type of easing to do for the envelope</param>
        public void ApplyVolumeEnvelope(float startVolume, float endVolume, float tweenDuration,
            Ease easeType = Ease.Linear)
        {
            if (musicSource == null) return;

            musicSource.volume = startVolume;
            DOTween.To(
                () => musicSource.volume,
                v => musicSource.volume = v,
                endVolume,
                tweenDuration
            ).SetEase(easeType).SetId(BGMTweenID);
        }

        /// <summary>
        /// Applies a specified pitch envelope to the current background music.
        /// </summary>
        /// <param name="startPitch">The initial pitch value (positive only)</param>
        /// <param name="endPitch">The final pitch value (positive only)</param>
        /// <param name="tweenDuration">The duration of the envelope in seconds</param>
        /// <param name="easeType">The type of easing to do for the envelope</param>
        public void ApplyPitchEnvelope(float startPitch, float endPitch, float tweenDuration,
            Ease easeType = Ease.Linear)
        {
            if (musicSource == null) return;

            startPitch = Mathf.Max(startPitch, 0.001f);
            endPitch = Mathf.Max(endPitch, 0.001f);

            // convert pitches to log2 base so re-pitching occurs linearly, not all at once
            float currPitchIndex = math.log2(startPitch);
            float endPitchIndex = math.log2(endPitch);
            
            musicSource.pitch = startPitch;
            DOTween.To(
                () => currPitchIndex,
                p => musicSource.pitch = Mathf.Pow(2.0f, p),
                endPitchIndex,
                tweenDuration
            ).SetEase(easeType).SetId(BGMTweenID);
        }
        
        /// <summary>
        /// Updates the highpass filter smoothly over a set duration of time.
        /// </summary>
        /// <param name="startFreq">The initial cutoff frequency for the highpass filter in Hz</param>
        /// <param name="targetFreq">The target cutoff frequency for the highpass filter in Hz</param>
        /// <param name="tweenDuration">The duration of the envelope in seconds</param>
        /// <param name="easeType">The type of easing to use for the envelope</param>
        public void ApplyHighpassFilterEnvelope(float startFreq, float targetFreq, float tweenDuration,
            Ease easeType = Ease.Linear)
        {
            if (highpassFilter == null) return;
            
            // convert lerped values to log scale to match frequency response
            var logStart = Mathf.Log10(startFreq);
            var logTarget = Mathf.Log10(targetFreq);
            
            DOTween.To(
                () => logStart,
                x => highpassFilter.cutoffFrequency = Mathf.Pow(10, x),
                logTarget,
                tweenDuration
            ).SetEase(easeType).SetId(BGMTweenID);
        }

        /// <summary>
        /// Updates the lowpass filter smoothly over a set duration of time.
        /// </summary>
        /// <param name="startFreq">The target cutoff frequency for the lowpass filter in Hz</param>
        /// <param name="targetFreq">The target cutoff frequency for the lowpass filter in Hz</param>
        /// <param name="tweenDuration">The duration of the envelope in seconds</param>
        /// <param name="easeType">The type of easing to use for the envelope</param>
        public void ApplyLowpassFilterEnvelope(float startFreq, float targetFreq, float tweenDuration,
            Ease easeType = Ease.Linear)
        {
            if (lowpassFilter == null) return;
            
            // convert lerped values to log scale to match frequency response
            var logStart = Mathf.Log10(startFreq);
            var logTarget = Mathf.Log10(targetFreq);
            
            DOTween.To(
                () => logStart,
                x => lowpassFilter.cutoffFrequency = Mathf.Pow(10, x),
                logTarget,
                tweenDuration
            ).SetEase(easeType).SetId(BGMTweenID);
        }

        /// <summary>
        /// Updates the reverb effect smoothly over a set duration of time.
        /// </summary>
        /// <param name="startLevel">The initial level for the reverb effect on [0.0, 1.0]</param>
        /// <param name="targetLevel">The target level for the reverb effect on [0.0, 1.0]</param>
        /// <param name="tweenDuration">The duration of the envelope in seconds</param>
        /// <param name="easeType">The type of easing to use for the envelope</param>
        public void ApplyReverbEnvelope(float startLevel, float targetLevel, float tweenDuration,
            Ease easeType = Ease.Linear)
        {
            if (reverbFilter == null) return;

            // clamp volume inputs to reasonable levels
            var currLevel = Mathf.Clamp(startLevel, 0.0f, 1.0f);
            targetLevel = Mathf.Clamp(targetLevel, 0.0f, 1.0f);

            // remap from [0,1] to [0.0625,1.0], which makes its logarithmic scaling on [-10,000,-1]
            currLevel = (currLevel + 0.0625f) / 1.0625f;
            targetLevel = (targetLevel + 0.0625f) / 1.0625f;

            // do tween, updating volume with function reverbLevel = 1-10^(-log2(vol)), which scales the volume
            // logarithmically on [-9999.0, 0.0]
            reverbFilter.reverbLevel = currLevel;
            DOTween.To(
                () => currLevel,
                v => reverbFilter.reverbLevel = 1.0f - Mathf.Pow(-math.log2(v), 10.0f),
                targetLevel,
                tweenDuration
            ).SetEase(easeType).SetId(BGMTweenID);
        }
    }
}