using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Props.SlotMachine
{
    /// <summary>
    /// Represents a Slot wheel in the Slot machine.
    /// </summary>
    public class SlotWheel : MonoBehaviour
    {
        /// <summary>
        /// Target end rotation for the wheel.
        /// </summary>
        [SerializeField]
        private float targetEndRotation = 100f;
        
        /// <summary>
        /// Speed at which the wheel spins.
        /// </summary>
        [SerializeField]
        private float spinSpeed = 1000f;
        
        /// <summary>
        /// Target rotation for the wheel.
        /// </summary>
        private Vector3 _targetRotation;
        
        /// <summary>
        /// Is the wheel spinning?
        /// </summary>
        private bool _spinning = false;

        private void Start()
        {
            // transform.localEulerAngles = new Vector3(targetEndRotation, 0, 0);
            _targetRotation = new Vector3(targetEndRotation, 0, 0);
            
            // Set Random start rotation
            var randomRotation = Random.Range(0, 360);
            transform.Rotate(Vector3.left, randomRotation);
        }

        /// <summary>
        /// Coroutine for spinning the wheel.
        /// </summary>
        /// <returns></returns>
        private IEnumerator SpinCoroutine()
        {
            _spinning = true;
            var spinSpeedVariation = spinSpeed * Random.Range(200f, 400f);
            while (_spinning)
            {
                // rotate on euler X
                transform.Rotate(Vector3.left, spinSpeedVariation * Time.deltaTime);
                yield return null;
            }
            
            transform.localEulerAngles = _targetRotation;
        }

        /// <summary>
        /// Moves the slot into the machine.
        /// </summary>
        public void HideSlot()
        {
            // Hide the slots by moving them back into the machine
            transform.DOLocalMoveZ(-0.02f, 0.5f).SetEase(Ease.InOutCubic);
        }

        /// <summary>
        /// Starts the wheel spinning.
        /// </summary>
        public void StartWheel()
        {
            StartCoroutine(SpinCoroutine());
        }

        /// <summary>
        /// Stops the wheel spinning.
        /// </summary>
        public void StopWheel()
        {
            _spinning = false;
        }
    }
}