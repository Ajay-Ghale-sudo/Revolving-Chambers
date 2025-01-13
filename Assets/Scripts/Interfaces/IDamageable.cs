using System;
using UnityEngine;
using UnityEngine.Events;

namespace Interfaces
{
    /// <summary>
    /// Interface for objects that can take damage.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Get the Event for when the object dies.
        /// </summary>
        /// <returns>The Event for OnDeath.</returns>
        public UnityEvent GetOnDeathEvent();
        
        /// <summary>
        /// Get the Event for when the object takes damage.
        /// </summary>
        /// <returns>The Event for OnDamage.</returns>
        public UnityEvent GetOnDamageEvent();
        /// <summary>
        /// Take damage.
        /// </summary>
        /// <param name="damage">Damage data to apply.</param>
        public void TakeDamage(DamageData damage);

        /// <summary>
        /// Play damage effects.
        /// </summary>
        /// <param name="colour">Colour to flash</param>
        public void PlayDamageEffect(Color colour);
    }

    /// <summary>
    /// Types of damage that can be applied.
    /// </summary>
    [Serializable]
    public enum DamageType
    {
        Unknown,
        Player,
        Boss,
        Environment,
    }
    
    /// <summary>
    /// Data for damage.
    /// </summary>
    [Serializable]
    public struct DamageData
    {
        public int damage;
        public DamageType type;
    }

    /// <summary>
    /// Base class for objects that can take damage.
    /// </summary>
    public class Damageable : MonoBehaviour, IDamageable
    {
        /// <summary>
        /// Event invoked when the object dies.
        /// </summary>
        public UnityEvent OnDeath;
        
        /// <summary>
        /// Event invoked when the object takes damage.
        /// </summary>
        public UnityEvent OnDamage;

        public UnityEvent GetOnDeathEvent()
        {
            return OnDeath;
        }

        public UnityEvent GetOnDamageEvent()
        {
            return OnDamage;
        }

        public virtual void TakeDamage(DamageData damage)
        {
        }

        public virtual void PlayDamageEffect(Color colour)
        {
        }
    }
}