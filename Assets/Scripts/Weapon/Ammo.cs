using System.Collections.Generic;
using Events;
using Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace Weapon
{
    /// <summary>
    /// Ammo for a weapon.
    /// </summary>
    [CreateAssetMenu(fileName = "data", menuName = "Scriptable Objects/Ammo", order = 0)]
    public class Ammo : ScriptableObject
    {
        /// <summary>
        /// The damage data used by this ammo.
        /// </summary>
        [SerializeField] public DamageData damage;
        
        /// <summary>
        /// The prefab for the projectile.
        /// </summary>
        [SerializeField]
        public GameObject projectilePrefab;

        /// <summary>
        /// The velocity of the projectile.
        /// </summary>
        [SerializeField] public float velocity = 100f;
        
        /// <summary>
        /// The lifetime of the projectile.
        /// </summary>
        [SerializeField]
        public float lifetime = 5f;
        
        /// <summary>
        /// The color of the ammo.
        /// </summary>
        [SerializeField] 
        public Color color = Color.white;
            
        /// <summary>
        /// Events to trigger when this bullet is destructed.
        /// </summary>
        [SerializeReference]
        public List<ScriptableObject> OnEndEvents;
    }
}