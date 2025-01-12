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
        public UnityEvent OnDeath { get; }
        public UnityEvent OnDamage { get; }
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
}