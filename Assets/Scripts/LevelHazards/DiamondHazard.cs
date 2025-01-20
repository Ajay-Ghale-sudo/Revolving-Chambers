using System;
using System.Collections;
using DG.Tweening;
using Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace LevelHazards
{
    /// <summary>
    /// Diamond hazard that moves towards the player and deals damage.
    /// </summary>
    public class DiamondHazard : MonoBehaviour
    {
        /// <summary>
        /// Event invoked when the hazard is activated.
        /// </summary>
        public UnityEvent OnHazardActivated;

        /// <summary>
        /// Flag to check if the hazard is active.
        /// </summary>
        private bool _hazardActive = false;

        /// <summary>
        /// Time to activate the hazard.
        /// </summary>
        [SerializeField] private float timeToActivate = 1.5f;

        /// <summary>
        /// Duration of the hazard.
        /// </summary>
        [SerializeField] private float duration = 5f;

        /// <summary>
        /// Speed at which the hazard moves.
        /// </summary>
        [SerializeField] private float moveSpeed = 7.5f;

        /// <summary>
        /// Interval at which the hazard deals damage.
        /// </summary>
        [SerializeField] private float damageInterval = 1.5f;

        /// <summary>
        /// Target to move towards.
        /// </summary>
        public Transform _target;

        /// <summary>
        /// Damage data for the hazard.
        /// </summary>
        [SerializeField] private DamageData damageData;

        /// <summary>
        /// Current damageable to deal damage to.
        /// </summary>
        private Damageable _currentDamageable;

        /// <summary>
        /// Child diamond object. Shown when hazard is deactivated.
        /// </summary>
        [SerializeField] private GameObject _childDiamond;

        /// <summary>
        /// Damage coroutine.
        /// </summary>
        private Coroutine _damageRoutine;

        private void Awake()
        {
        }

        void Start()
        {
            Invoke(nameof(ActivateHazard), timeToActivate);
            _target = GameObject.FindGameObjectWithTag("Player").transform;
        }

        private void Update()
        {
            // If not active, move towards target, else do nothing 
            if (_hazardActive || !transform) return;

            transform.position = Vector3.MoveTowards(transform.position, _target.position, moveSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Activates the hazard.
        /// </summary>
        private void ActivateHazard()
        {
            _hazardActive = true;
            OnHazardActivated?.Invoke();
            
            Invoke(nameof(DeactivateHazard), 0.5f);
        }

        /// <summary>
        /// Deactivates the hazard.
        /// </summary>
        private void DeactivateHazard()
        {
            _currentDamageable?.TakeDamage(damageData);
            _childDiamond?.SetActive(true);
            transform.DOScale(Vector3.zero, duration).OnComplete(() => Destroy(gameObject)).SetEase(Ease.InBounce);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _currentDamageable = other.GetComponent<Damageable>();
                StopDamageCoroutine();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _currentDamageable = null;
                StopDamageCoroutine();
            }
        }

        /// <summary>
        /// Stops the damage coroutine.
        /// </summary>
        private void StopDamageCoroutine()
        {
            if (_damageRoutine == null) return;
            StopCoroutine(_damageRoutine);
            _damageRoutine = null;
        }

        /// <summary>
        /// Coroutine to deal damage to the player.
        /// </summary>
        /// <returns></returns>
        private IEnumerator DamageRoutine()
        {
            while (_currentDamageable)
            {
                if (!_hazardActive) yield break;
                _currentDamageable.TakeDamage(damageData);
                yield return new WaitForSeconds(damageInterval);
            }
        }
    }
}