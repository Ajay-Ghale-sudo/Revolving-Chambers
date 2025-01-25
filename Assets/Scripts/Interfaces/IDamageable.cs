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
        
        /// <summary>
        /// Health of the object.
        /// </summary>
        [SerializeField]
        protected float health = 100;
        
        /// <summary>
        /// Health of the object.
        /// </summary>
        public float Health => health;
        
        /// <summary>
        /// Maximum health of the object.
        /// </summary>
        [SerializeField]
        protected float maxHealth = 100;
        
        /// <summary>
        /// Maximum health of the object.
        /// </summary>
        public float MaxHealth => maxHealth;
        
        /// <summary>
        /// Flag to check if damage is enabled.
        /// </summary>
        protected bool _damageEnabled = true;
        
        public UnityEvent GetOnDeathEvent()
        {
            return OnDeath;
        }

        public UnityEvent GetOnDamageEvent()
        {
            return OnDamage;
        }
        
        
        /// <summary>
        /// Enable damage.
        /// </summary>
        public void EnableDamage()
        {
            _damageEnabled = true;
        }

        
        /// <summary>
        /// Disable damage.
        /// </summary>
        public void DisableDamage()
        {
            _damageEnabled = false;
        }

        public virtual void TakeDamage(DamageData damage)
        {
            if (!_damageEnabled) return;
            
            health -= damage.damage;
            OnDamage?.Invoke();
            if (health <= 0)
            {
                Die();
            }
        }

        public virtual void PlayDamageEffect(Color colour)
        {
        }

        protected virtual void Die()
        {
            OnDeath?.Invoke();
        }
    }
}