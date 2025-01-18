using System;
using Props;
using State;
using UI;
using UnityEngine;
using Weapon;

namespace Boss
{
    /// <summary>
    /// Attack data for the boss.
    /// </summary>
    [Serializable]
    public struct TargetDummyBossAttackData
    {
        /// <summary>
        /// Ammo to fire.
        /// </summary>
        public Ammo ammo;

        /// <summary>
        /// Rate of fire.
        /// </summary>
        public float fireRate;

        /// <summary>
        /// Variation in fire rate.
        /// </summary>
        public float fireVariation;

        /// <summary>
        /// Cooldown for the attack.
        /// </summary>
        public float cooldown { get; set; }
    }

    /// <summary>
    /// Boss Target Dummy.
    /// </summary>
    [RequireComponent(typeof(TargetDummy))]
    public class BossTargetDummy : MonoBehaviour
    {
        /// <summary>
        /// Tag of the target to shoot at.
        /// </summary>
        [SerializeField] private string targetTag = "Player";

        /// <summary>
        /// Transform of the target to shoot at.
        /// </summary>
        [SerializeField] private Transform target;

        /// <summary>
        /// Primary attack data for the boss.
        /// </summary>
        [SerializeField] private TargetDummyBossAttackData primaryAttackData;

        /// <summary>
        /// Secondary attack data for the boss.
        /// </summary>
        [SerializeField] private TargetDummyBossAttackData secondaryAttackData;

        /// <summary>
        /// Target dummy component.
        /// </summary>
        private TargetDummy _targetDummy;
        // private float _fireCooldown = 0f;
        // private float _secondaryFireCooldown = 0f;

        private void Awake()
        {
            _targetDummy = GetComponent<TargetDummy>();
        }

        private void Start()
        {
            UIManager.Instance.OnBossSpawned?.Invoke(name);
            UIManager.Instance.OnBossHealthChange?.Invoke(_targetDummy.Health / _targetDummy.MaxHealth);
            _targetDummy.OnDamage?.AddListener(HandleDamage);
            _targetDummy?.OnDeath?.AddListener(Die);
            
            UpdateTarget();

        }
        
        private void OnDestroy()
        {
            _targetDummy?.OnDamage?.RemoveListener(HandleDamage);
            _targetDummy?.OnDeath?.RemoveListener(Die);
        }

        private void Update()
        {
            // TODO: Have these be coroutines that run every x seconds
            ShootAtTarget(ref primaryAttackData);
            ShootAtTarget(ref secondaryAttackData);
        }

        private void Die()
        {
            GameStateManager.Instance.OnBossDeath?.Invoke();
        }
        
        /// <summary>
        /// Process damage taken.
        /// </summary>
        private void HandleDamage()
        {
            UIManager.Instance.OnBossHealthChange?.Invoke(_targetDummy.Health / _targetDummy.MaxHealth);
        }

        /// <summary>
        /// Update the target to shoot at.
        /// </summary>
        private void UpdateTarget()
        {
            // Find target from tag
            GameObject targetObject = GameObject.FindGameObjectWithTag(targetTag);
            if (targetObject != null)
            {
                target = targetObject.transform;
            }
            else
            {
                Debug.LogWarning("No target found with tag: " + targetTag);
            }
        }

        /// <summary>
        /// Shoot at the target.
        /// </summary>
        /// <param name="attackData">Attack data to fire at player</param>
        /// <returns>The <see cref="Bullet"/> created</returns>
        private Bullet ShootAtTarget(ref TargetDummyBossAttackData attackData)
        {
            attackData.cooldown = Mathf.Max(0, attackData.cooldown - Time.deltaTime);
            if (!target || !attackData.ammo || attackData.cooldown > 0) return null;

            // Spawn projectile outside of the boss
            var spawnPosition = _targetDummy.GetComponent<Collider>().bounds.center;
            // Offset spawn position towards target
            spawnPosition += (transform.position - target.position).normalized * 2f;

            var directionToTarget = (target.position - spawnPosition).normalized;
            var projectile = BulletManager.Instance.SpawnBullet(attackData.ammo, spawnPosition,
                Quaternion.LookRotation(directionToTarget));
            attackData.cooldown = attackData.fireRate +
                                  UnityEngine.Random.Range(-attackData.fireVariation, attackData.fireVariation);
            return projectile;
        }
    }
}