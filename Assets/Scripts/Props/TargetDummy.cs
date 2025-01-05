using Interfaces;
using UnityEngine;

namespace Props
{
    /// <summary>
    /// Target dummy that can take damage.
    /// </summary>
    public class TargetDummy : MonoBehaviour, IDamageable
    {
        /// <summary>
        /// The health of the target dummy.
        /// </summary>
        [SerializeField]
        private float health = 20f;
        
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
        /// Destroy the target dummy.
        /// </summary>
        private void Die()
        {
            Destroy(gameObject);
        }
    }
}