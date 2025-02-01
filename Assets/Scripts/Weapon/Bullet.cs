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
        /// Trail renderer on the bullet
        /// </summary>
        [SerializeField] private TrailRenderer _trailRenderer;

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
        /// Instanced material that should be destroyed manually.
        /// Used for changing this object's material through script.
        /// </summary>
        Material _instancedMaterial;

        /// <summary>
        /// The object that was hit by the bullet.
        /// </summary>
        public GameObject ObjectHit { get; private set; }
        
        /// <summary>
        /// Whether the bullet is destroyed by bullet collision.
        /// </summary>
        [SerializeField] private bool destroyedByBulletCollision = true;

        /// <summary>
        /// Set the ammo for the bullet.
        /// </summary>
        /// <param name="ammo">The new ammo type</param>
        public void SetAmmo(Ammo ammo)
        {
            Ammo = ammo;

            SetColor(ammo.color);

            // process lifetime
            if (Ammo.lifetime > 0)
            {
                Invoke(nameof(Destruct), Ammo.lifetime);
            }
        }

        /// <summary>
        /// Changes the colour of the trail renderer
        /// TODO: change bullet colour
        /// </summary>
        /// <param name="colour">New colour</param>
        public void SetColor(Color colour)
        {
            if (_trailRenderer == null) return;

            //Clone the material and start using it from now on
            _instancedMaterial = _trailRenderer.material;

            //Set emission and colour on instanced material
            _instancedMaterial.EnableKeyword("_EMISSION");
            _instancedMaterial.SetColor("_EmissionColor", colour);
            _instancedMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
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
            
            ObjectHit = other.gameObject;
            
            OnHit?.Invoke(this);
            if (other.gameObject.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(Ammo.damage);
            }
           
            if (!destroyedByBulletCollision && other.gameObject.TryGetComponent<Bullet>(out var bullet))
            {
                return;
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
            //Instanced materials are not managed by Unity
            //Make sure to destroy the material
            Destroy(_instancedMaterial);
        }
    }
}