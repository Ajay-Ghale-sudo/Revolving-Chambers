using Events;
using UnityEngine;
using UnityEngine.Events;
using Interfaces;

namespace LevelHazards
{
    /// <summary>
    /// Script for controlling the falling poker chip attack
    /// </summary>
    public class FallingChipHazard : MonoBehaviour
    {
        /// <summary>
        /// Event that is invoked when the attack is completed
        /// </summary>
        public UnityEvent OnLaunch;

        /// <summary>
        /// Event that is invoked when the attack is completed
        /// </summary>
        public UnityEvent OnFinished;

        /// <summary>
        /// Animator for hte falling object
        /// </summary>
        [SerializeField] private Animator _animator;

        /// <summary>
        /// ParticleSystem for falling object impact
        /// </summary>
        [SerializeField] private ParticleSystem _impactParticleSystem;

        /// <summary>
        /// Damage data for the hazard.
        /// </summary>
        [SerializeField] private DamageData _damageData;

        [SerializeField] private AudioEvent PlayChipLandThunkAudioEvent;

        /// <summary>
        /// Track if triggered.
        /// Only trigger once per launch
        /// </summary>
        private bool _triggered = false;

        /// <summary>
        /// Launch the falling hazard effect.
        /// Pretty much everything is driven by animator for this effect.
        /// </summary>
        public void Launch()
        {
            _triggered = false;

            if (_animator == null) return;

            gameObject.SetActive(true);
            _animator.SetTrigger("Launch");
            OnLaunch?.Invoke();
        }

        /// <summary>
        /// Emit a one time burst of particles with the particle system
        /// </summary>
        /// <param name="count">particles to emit</param>
        public void PlayParticleEffect(int count = 5)
        {
            if (_impactParticleSystem == null) return;

            _impactParticleSystem.Emit(count);

                PlayChipLandThunkAudioEvent?.Invoke();
        }

        /// <summary>
        /// Called by the animator when the falling animation ends
        /// </summary>
        public void OnAttackEnd()
        {
            OnFinished?.Invoke();

            gameObject.SetActive(false);
        }

        /// <summary>
        /// Detect player Damageable component and deal damage if found.
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (!_triggered && other.CompareTag("Player"))
            {
                if(other.TryGetComponent(out Damageable script))
                {
                    script.TakeDamage(_damageData);
                    _triggered = true;
                }
            }
        }
    }
}
