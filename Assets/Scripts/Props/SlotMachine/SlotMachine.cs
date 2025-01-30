using System;
using System.Collections.Generic;
using Audio;
using DG.Tweening;
using Events;
using UnityEngine;
using UnityEngine.Serialization;

namespace Props.SlotMachine
{
    /// <summary>
    /// Represents a Slot machine.
    /// </summary>
    public class SlotMachine : MonoBehaviour
    {
        /// <summary>
        /// List of slot wheels in the machine.
        /// </summary>
        private List<SlotWheel> _slotWheels;
        
        /// <summary>
        /// Attached camera to the slot machine.
        /// </summary>
        private Camera _camera;
        
        [SerializeField]
        private float cameraEndFOV = 20f;

        /// <summary>
        /// Where the camera should end up after the slot machine stops.
        /// </summary>
        [SerializeField]
        private float cameraEndPositionZ = 0.23f;
        
        /// <summary>
        /// Event to invoke when the slot machine stops.
        /// </summary>
        [SerializeField]
        private GameEvent _onSlotMachineStopped;
        
        /// <summary>
        /// Animation curve for the slot machine lever.
        /// </summary>
        [SerializeField]
        private AnimationCurve _SlotLeverCurve;
        
        /// <summary>
        /// Rotation of the lever when pulled.
        /// </summary>
        [SerializeField]
        private float leverRotation = 45f;
        
        /// <summary>
        /// Lever transform.
        /// </summary>
        [SerializeField]
        private Transform _lever;
        
        /// <summary>
        /// Tween for the lever.
        /// </summary>
        private Tweener _leverTween;

        /// <summary>
        /// Event to play audio for the intro sequence.
        /// </summary>
        [SerializeField] private AudioEvent IntroAudioEvent;

        /// <summary>
        /// Audio clip for the intro background music.
        /// </summary>
        [SerializeField] private AudioClip IntroBackgroundMusic;

        private void Awake()
        {
            _slotWheels = new List<SlotWheel>(GetComponentsInChildren<SlotWheel>());
            _camera = GetComponentInChildren<Camera>();
            AudioManager.Instance?.SetBackgroundMusic(IntroBackgroundMusic, 1.0f);
        }

        private void Start()
        {
        }

        /// <summary>
        /// Starts the slot machine spinning.
        /// </summary>
        public void StartSpin()
        {
            _leverTween?.PlayBackwards();
            foreach (var wheel in _slotWheels)
            {
                wheel.StartWheel();
            }
            
            Invoke(nameof(StopSpin), 1.5f);
        }

        /// <summary>
        /// Pulls the lever to start the slot machine.
        /// </summary>
        public void PullLevel()
        {
            // Pull the lever to the animation curve and then start spinning the wheel
            // Rotate by rotating X
            _leverTween = _lever.DOLocalRotate(new Vector3(leverRotation, 0, 0), 0.5f)
                .SetEase(_SlotLeverCurve)
                .SetAutoKill(false)
                .OnComplete(StartSpin);
            
            AudioManager.Instance?.StartBackgroundMusicLowpassFilterEnvelope(1000.0f, 250.0f, 1.5f);
            AudioManager.Instance?.StartBackgroundMusicVolumeEnvelope(1.0f, 0.0f, 5.0f);
            IntroAudioEvent?.Invoke();
        }

        /// <summary>
        /// Stops the slot machine spinning.
        /// </summary>
        public void StopSpin()
        {
            // Stop each wheel one by one with a delay
            for (var i = 0; i < _slotWheels.Count; i++)
            {
                var wheel = _slotWheels[i];
                var delay = i * 0.5f;
                wheel.Invoke(nameof(wheel.StopWheel), delay);
                wheel.Invoke(nameof(wheel.HideSlot), 2.5f);
            }
            
            Invoke(nameof(DragInCamera), 3.5f);
        }

        /// <summary>
        /// Drag the camera in after the slot machine stops.
        /// </summary>
        private void DragInCamera()
        {
            // Move the camera to end position
            _camera.transform.DOLocalMoveZ(cameraEndPositionZ, 1f).SetEase(Ease.InOutCubic);
            _camera.DOFieldOfView(cameraEndFOV, 1f).SetEase(Ease.InOutCubic).OnComplete(() =>
            {
                _onSlotMachineStopped?.Invoke(gameObject);
            });
        }
    }
}