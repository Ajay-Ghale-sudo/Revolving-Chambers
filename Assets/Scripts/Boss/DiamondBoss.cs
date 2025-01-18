using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Interfaces;
using State;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;
using Weapon;

namespace Boss
{
    /// <summary>
    /// Data for the diamond boss attack.
    /// </summary>
    [Serializable]
    public struct DiamondBossAttackData
    {
        /// <summary>
        /// The ammo the attack uses.
        /// </summary>
        public Ammo ammo;
        
        /// <summary>
        /// The fire rate of the attack.
        /// </summary>
        public float fireVariation;
        
        /// <summary>
        ///  The amount of cooldown left
        /// </summary>
        public float cooldown;
    }
    public class DiamondBoss : Damageable 
    {

        /// <summary>
        /// The spawn targets for the projectiles. 
        /// </summary>
        [SerializeField] 
        public List<Transform> projectileSpawnTargets;
        
        /// <summary>
        /// Spawn targets for the diagonal projectiles.
        /// </summary>
        [SerializeField]
        public List<Transform> projectileDiagSpawnTargets;
        
        /// <summary>
        /// The attack data for the boss.
        /// </summary>
        public DiamondBossAttackData attackData;

        /// <summary>
        /// Whether the boss is currently running an attack pattern.
        /// </summary>
        private bool _running;
        
        /// <summary>
        /// Controls which attack pattern the boss is using.
        /// </summary>
        private bool _useAltAttackPattern;

        /// <summary>
        /// The spline for the boss to follow.
        /// </summary>
        [SerializeField]
        public SplineContainer spline;
        
        /// <summary>
        /// Cached length of the current spline
        /// </summary>
        private float _splineLength = 0f;
        
        /// <summary>
        /// The movement speed of the boss.
        /// </summary>
        public float moveSpeed = 1f;
        
        /// <summary>
        /// The current distance along the spline.
        /// </summary>
        private float _currentDistance = 0f;

        
        /// <summary>
        /// Time between boss attacks.
        /// </summary>
        [SerializeField] private float attackInterval = 2f;

        /// <summary>
        /// The rotation vector for the boss.
        /// </summary>
        [SerializeField] private Vector3 rotationVector = new Vector3(0f, 360f, 0f);

        /// <summary>
        /// The duration of the rotation. Shorter duration, faster spin.
        /// </summary>
        [SerializeField] private float rotationDuration = 1f;
        
        /// <summary>
        /// The mode of rotation for the boss.
        /// </summary>
        [SerializeField] private RotateMode rotateMode = RotateMode.FastBeyond360;

        
        void Start()
        {
            transform.DORotate(rotationVector, rotationDuration, rotateMode)
                .SetLoops(-1).SetRelative(true).SetEase(Ease.Linear);
            StartCoroutine(AttackPatternCoroutine());

            _splineLength = spline.CalculateLength();
            
            UIManager.Instance.OnBossSpawned?.Invoke(name);
            UIManager.Instance.OnBossHealthChange?.Invoke(health / maxHealth);

        }
        void Update()
        {
            // Calculate the target position on the spline.
            // Sets the world position for the player to move to calculated by the normalized value currentDistance.
            Vector3 targetPosition = spline.EvaluatePosition(_currentDistance);
            
            // Move the character towards the target position on the spline.
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            
            // calculate how far along we are 0 -> 1.0
            _currentDistance = (_currentDistance + ((moveSpeed * Time.deltaTime) / _splineLength)) % 1f;
        }

        /// <summary>
        /// Coroutine for the attack pattern.
        /// </summary>
        /// <returns></returns>
        IEnumerator AttackPatternCoroutine()
        {
            _running = true;
            while (_running)
            {
                SpawnBulletPattern();
                _useAltAttackPattern = !_useAltAttackPattern;
                Invoke(nameof(SpawnBulletPattern), .5f);
                yield return new WaitForSeconds(attackInterval);
            }
        }

        /// <summary>
        /// Spawns the bullet pattern for the current attack pattern.
        /// </summary>
        void SpawnBulletPattern()
        {
            var targets = _useAltAttackPattern ? projectileSpawnTargets : projectileDiagSpawnTargets;
            foreach (var target in targets)
            {
                var spawnPosition = target.position;
                var direction = target.position - transform.position;
                var projectile = BulletManager.Instance.SpawnBullet(attackData.ammo, spawnPosition, Quaternion.LookRotation(direction));
            }
        }


        public override void TakeDamage(DamageData damage)
        {
            if (damage.type != DamageType.Player) return;
            health -= damage.damage;
            OnDamage?.Invoke();
            UIManager.Instance.OnBossHealthChange?.Invoke(health / maxHealth);
            PlayDamageEffect(Color.red);
            if (health > 0) return;
            Die();
        }

        protected override void Die()
        {
            _running = false;
            OnDeath?.Invoke();
            GameStateManager.Instance.OnBossDeath?.Invoke();
            Destroy(gameObject);
        }
    }
}
