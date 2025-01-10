using UnityEngine;

namespace Interfaces
{
    /// <summary>
    /// Interface for objects that can take damage.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Take damage.
        /// </summary>
        /// <param name="damage">Amount of damage to apply</param>
        public void TakeDamage(int damage);

        /// <summary>
        /// Play damage effects.
        /// TODO: ammo/hit data can go here
        /// </summary>
        /// <param name="colour">Colour to flash</param>
        public void PlayDamageEffect(Color colour);
    }
}