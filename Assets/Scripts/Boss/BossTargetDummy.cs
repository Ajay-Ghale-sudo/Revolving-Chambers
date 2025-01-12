using System;
using Props;
using UnityEngine;
using Weapon;

namespace Boss
{
    [Serializable]
    public struct TargetDummyBossAttackData
    {
        public Ammo ammo;
        public float fireRate;
        public float fireVariation;
        
        public float cooldown;
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
        // [SerializeField] private Ammo primaryAmmo;
        // [SerializeField] private float fireRate = 1f;
        // [SerializeField] private float fireVariation = 0.3f;
        
        /// <summary>
        /// Primary attack data for the boss.
        /// </summary>
        [SerializeField] private TargetDummyBossAttackData primaryAttackData;
        
        /// <summary>
        /// Secondary attack data for the boss.
        /// </summary>
        [SerializeField] private TargetDummyBossAttackData secondaryAttackData;
        
        // [SerializeField] private Ammo secondaryAmmo;
        // [SerializeField] private float secondaryFireRate = 5f;
        // [SerializeField] private float secondaryFireVariation = 1.3f;
        
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
            UpdateTarget();
        }

        private void Update()
        {
            // TODO: Have these be coroutines that run every x seconds
            // if (ShootPrimaryAtTarget()) return;
            // ShootSecondaryAtTarget();
            ShootAtTarget(ref primaryAttackData);
            ShootAtTarget(ref secondaryAttackData);
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
            var projectile = BulletManager.Instance.SpawnBullet(attackData.ammo, spawnPosition, Quaternion.LookRotation(directionToTarget));
            attackData.cooldown = attackData.fireRate + UnityEngine.Random.Range(-attackData.fireVariation, attackData.fireVariation);
            return projectile;
        }

        // private Bullet ShootPrimaryAtTarget()
        // {
        //     _fireCooldown = Mathf.Max(0, _fireCooldown - Time.deltaTime);
        //     if (!target || !primaryAmmo || _fireCooldown > 0) return null;
        //     
        //     // Spawn projectile outside of the boss
        //     var spawnPosition = _targetDummy.GetComponent<Collider>().bounds.center;
        //     // Offset spawn position towards target
        //     spawnPosition += (transform.position - target.position).normalized * 2f;
        //     
        //     var directionToTarget = (target.position - spawnPosition).normalized;
        //     var projectile = BulletManager.Instance.SpawnBullet(primaryAmmo, spawnPosition, Quaternion.LookRotation(directionToTarget));
        //     _fireCooldown = fireRate + UnityEngine.Random.Range(-fireVariation, fireVariation);
        //     return projectile;
        // }
        //
        // private Bullet ShootSecondaryAtTarget()
        // {
        //     _secondaryFireCooldown = Mathf.Max(0, _secondaryFireCooldown - Time.deltaTime);
        //     if (!target || !secondaryAmmo || _secondaryFireCooldown > 0) return null;
        //     Debug.Log("Shooting secondary at target: " + target.name);
        //     
        //     // Spawn projectile outside of the boss
        //     var spawnPosition = _targetDummy.GetComponent<Collider>().bounds.center;
        //     // Offset spawn position towards target
        //     spawnPosition += (transform.position - target.position).normalized * 2f;
        //     
        //     var directionToTarget = (target.position - spawnPosition).normalized;
        //     // Add slight variation to the direction
        //     directionToTarget += UnityEngine.Random.insideUnitSphere * 0.1f;
        //     directionToTarget.y = 0;
        //     directionToTarget.Normalize();
        //     
        //     var projectile = BulletManager.Instance.SpawnBullet(secondaryAmmo, spawnPosition, Quaternion.LookRotation(directionToTarget));
        //     _secondaryFireCooldown = secondaryFireRate + UnityEngine.Random.Range(-secondaryFireVariation, secondaryFireVariation);
        //     return projectile;
        // }
    }
}