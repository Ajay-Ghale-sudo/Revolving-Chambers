using UnityEngine;

namespace Player
{
    /// <summary>
    /// Script that sets the perameters of the character's animator
    /// </summary>
    public class CharacterAnimationHandler : MonoBehaviour
    {
        /// <summary>
        /// Animator to control. Set in Editor
        /// </summary>
        [SerializeField] private Animator _animator;

        /// <summary>
        /// Patricle System to control. Set in Editor
        /// </summary>
        [SerializeField] private ParticleSystem _particleSystem;

        /// <summary>
        /// Sets the "Forward" value
        /// +1 = forward; -1 = backward
        /// </summary>
        /// <param name="value">New value</param>
        public void SetForward(float value)
        {
            if (_animator == null) return;

            _animator.SetFloat("Forward", value);
        }

        /// <summary>
        /// Sets the "Right" value
        /// +1 = right; -1 = left
        /// </summary>
        /// <param name="value">New value</param>
        public void SetRight(float value)
        {
            if (_animator == null) return;

            _animator.SetFloat("Right", value);
        }

        /// <summary>
        /// Sets the trigger for "Dash"
        /// </summary>
        public void Play_Dash()
        {
            if (_animator == null) return;

            _animator.SetTrigger("Dash");
        }

        /// <summary>
        /// Sets the trigger for "Fire"
        /// </summary>
        public void Play_Shoot()
        {
            if (_animator == null) return;

            _animator.SetTrigger("Fire");
        }

        /// <summary>
        /// Tells the particle system to emit once. 
        /// Called by the animator in an animator event. 
        /// </summary>
        /// <param name="count">Number of particles to emit</param>
        public void Play_ShootParticles(int count = 3)
        {
            if (_particleSystem == null) return;

            _particleSystem.Emit(count);
        }
    }
}
