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
        /// The damage the ammo does.
        /// </summary>
        [SerializeField]
        public int damage = 10;
        
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
    }
}