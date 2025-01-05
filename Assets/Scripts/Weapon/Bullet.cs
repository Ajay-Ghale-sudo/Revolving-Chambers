using System;
using Interfaces;
using UnityEngine;

namespace Weapon
{
    /// <summary>
    /// Bullet fired by a weapon.
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        /// <summary>
        /// Event invoked when the bullet hits something.
        /// </summary>
        public static Action<Bullet> OnHit;
        
        /// <summary>
        /// Ammo data for the bullet.
        /// </summary>
        public Ammo Ammo { get; private set; }
        
        /// <summary>
        /// Set the ammo for the bullet.
        /// </summary>
        /// <param name="ammo">The new ammo type</param>
        public void SetAmmo(Ammo ammo)
        {
            Ammo = ammo;
        }

        /// <summary>
        /// Called when the bullet collides with something.
        /// </summary>
        /// <param name="other">The object the bullet collided with</param>
        private void OnCollisionEnter(Collision other)
        {
           OnHit?.Invoke(this);
           if (other.gameObject.TryGetComponent(out IDamageable damageable))
           {
               damageable.TakeDamage(Ammo.damage);
           }
           
           Destroy(gameObject);
        }
    }
}