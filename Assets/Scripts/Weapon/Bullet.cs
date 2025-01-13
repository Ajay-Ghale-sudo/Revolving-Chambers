using System;
using Events;
using Interfaces;
using UnityEngine;
using UnityEngine.Events;

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
        /// Event invoked when the bullet's lifetime ends.
        /// </summary>
        public UnityEvent OnEnd;
        
        /// <summary>
        /// Set the ammo for the bullet.
        /// </summary>
        /// <param name="ammo">The new ammo type</param>
        public void SetAmmo(Ammo ammo)
        {
            Ammo = ammo;
            
            // process lifetime
            if (Ammo.lifetime > 0)
            {
                Invoke(nameof(Destruct), Ammo.lifetime);
            }
        }

        /// <summary>
        /// Called when the bullet collides with something.
        /// </summary>
        /// <param name="other">The object the bullet collided with</param>
        private void OnCollisionEnter(Collision other)
        {
            if (Ammo == null)
            {
                Debug.LogWarning("Bullet has no ammo type.");
                Destruct();
                return;
            }
            
            OnHit?.Invoke(this);
            if (other.gameObject.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(Ammo.damage);
            }
           
            Destruct(); 
        }

        /// <summary>
        /// Destruct the bullet.
        /// </summary>
        public void Destruct()
        {
            if (Ammo?.OnEndEvents != null)
                foreach (var endEvent in Ammo.OnEndEvents)
                {
                    if (endEvent is not IGameEvent gameEvent) continue;
                    Debug.Log("Invoking game event " + gameEvent);
                    if (gameObject)
                        gameEvent.Invoke(gameObject);
                }

            OnEnd?.Invoke();
            enabled = false;
            gameObject.SetActive(false);
            Destroy(gameObject, 1.5f);
        }

        private void OnDisable()
        {
        }

        private void OnDestroy()
        {
        }
    }
}