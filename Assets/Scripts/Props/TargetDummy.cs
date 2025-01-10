using Interfaces;
using UnityEngine;
using System.Collections;

namespace Props
{
    /// <summary>
    /// Target dummy that can take damage.
    /// </summary>
    [RequireComponent(typeof(DamageableVFX))]
    public class TargetDummy : MonoBehaviour, IDamageable
    {
        /// <summary>
        /// The health of the target dummy.
        /// </summary>
        [SerializeField]
        private float health = 20f;

        /// <summary>
        /// VFX script attached to this gameobject
        /// </summary>
        DamageableVFX _vfxPlayer;

        void Start()
        {
            _vfxPlayer = GetComponent<DamageableVFX>();
        }

        /// <summary>
        /// Take damage.
        /// </summary>
        /// <param name="damage">Amount of damage to take</param>
        public void TakeDamage(int damage)
        {
            health -= damage;
            if (health > 0) return;
            Die();
        }

        /// <summary>
        /// Plays damage effects
        /// </summary>
        public void PlayDamageEffect(Color colour)
        {
            if (_vfxPlayer == null) return;

            _vfxPlayer.PlayFlashColour(colour, 0.1f);
        }

        /// <summary>
        /// Destroy the target dummy.
        /// </summary>
        private void Die()
        {
            Destroy(gameObject);
        }
        
    }
}