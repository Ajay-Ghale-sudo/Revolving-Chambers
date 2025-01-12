using Interfaces;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Weapon;

namespace Props
{
    /// <summary>
    /// Target dummy that can take damage.
    /// </summary>
    [RequireComponent(typeof(DamageableVFX))]
    public class TargetDummy : MonoBehaviour, IDamageable
    {
        /// <summary>
        /// The health of the target dummy.
        /// </summary>
        [SerializeField]
        private float health = 20f;

        /// <summary>
        /// VFX script attached to this gameobject
        /// </summary>
        DamageableVFX _vfxPlayer;
        
        /// <summary>
        /// The ammo to fire. If null, the target dummy will not fire.
        /// </summary>
        [SerializeField] private Ammo ammo;
        
        /// <summary>
        /// The rate at which to fire projectiles.
        /// </summary>
        [SerializeField]
        private float fireRate = 1f;

        /// <summary>
        /// The number of projectiles to fire around the target dummy.
        /// </summary>
        [SerializeField]
        private int projectileCount = 5;

        void Start()
        {
            _vfxPlayer = GetComponent<DamageableVFX>();

            if (ammo)
            {
                StartCoroutine(FireCoroutine());
            }
        }

        /// <summary>
        /// Event invoked when the target dummy dies.
        /// </summary>
        public UnityEvent OnDeath { get; set; }
        
        /// <summary>
        /// Event invoked when the target dummy takes damage.
        /// </summary>
        public UnityEvent OnDamage { get; set;  }

        /// <summary>
        /// Take damage.
        /// </summary>
        /// <param name="damage">Amount of damage to take</param>
        public void TakeDamage(DamageData damage)
        {
            if (damage.type != DamageType.Player) return;
            health -= damage.damage;
            OnDamage?.Invoke();
            PlayDamageEffect(Color.red);
            if (health > 0) return;
            Die();
        }

        /// <summary>
        /// Plays damage effects
        /// </summary>
        public void PlayDamageEffect(Color colour)
        {
            if (_vfxPlayer == null) return;

            _vfxPlayer.PlayFlashColour(colour, 0.1f);
        }

        /// <summary>
        /// Coroutine to fire projectiles at a set rate.
        /// </summary>
        /// <returns></returns>
        private IEnumerator FireCoroutine()
        {
            while (true)
            {
                Fire();
                yield return new WaitForSeconds(fireRate);
            }
        }
        
        /// <summary>
        /// Fire projectiles around the Target Dummy.
        /// </summary>
        private void Fire()
        {
           // Spawn projectiles all around the target dummy
              for (var index = 0; index < projectileCount; index++)
              {
                var angle = index * Mathf.PI * 2 / projectileCount;
                var dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                
                // Spawn position should be outside the target dummy
                var spawnPos = transform.position + dir * 2;
                var bullet = BulletManager.Instance.SpawnBullet(ammo, spawnPos, Quaternion.LookRotation(dir));
              }
        }

        /// <summary>
        /// Destroy the target dummy.
        /// </summary>
        private void Die()
        {
            OnDeath?.Invoke();
            Destroy(gameObject);
        }
        
    }
}