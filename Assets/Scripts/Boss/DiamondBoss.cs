using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Interfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;
using Weapon;

namespace Boss
{
    [Serializable]
    public struct DiamondBossAttackData
    {
        public Ammo ammo;
        public float fireVariation;
        public float cooldown;
    }
    public class DiamondBoss : MonoBehaviour, IDamageable
    {

        // List of transforms for the projectiles to spawn at.
        [SerializeField] 
        public List<Transform> projectileSpawnTargets;
        [SerializeField]
        public List<Transform> projectileDiagSpawnTargets;
        
        // Projectile attack data.
        public DiamondBossAttackData attackData;

        private float maxSpeed = 0.25f;
        private float currentTime = 0;
        private bool running;
        private bool testPattern;

        public AnimationCurve speedCurve;
        
        // Since spline animate is being annoying we'll write our own.
        [SerializeField]
        public SplineContainer spline;
        public float moveSpeed = 1f;
        private float currentDistance = 0f;
        private float movement = 0f;

        void Start()
        {
            transform.DORotate
                    (new Vector3(0f, 360.0f, 0.0f), 1f, RotateMode.FastBeyond360)
                .SetLoops(-1).SetRelative(true).SetEase(Ease.Linear);
            StartCoroutine(AttackPatternCoroutine());

        }
        void Update()
        {
            // Calculate the target position on the spline.
            // Sets the world position for the player to move to calculated by the normalized value currentDistance.
            Vector3 targetPosition = spline.EvaluatePosition(currentDistance);
            
            // Move the character towards the target position on the spline.
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            
            // If the end of the spline is reached, go back to the beginning.
            if (currentDistance >= 1f)
            {
                currentDistance = 0f;
            }
            else
            {
                // Adjust the movement based on the length of the spline
                float splineLength = spline.CalculateLength();
                float movement = moveSpeed * Time.deltaTime / splineLength;
                currentDistance += movement;
            }
        }

        IEnumerator AttackPatternCoroutine()
        {
            running = true;
            while (running)
            {
                SpawnBulletPattern();
                testPattern = !testPattern;
                Invoke(nameof(SpawnBulletPattern), .5f);
                yield return new WaitForSeconds(2);
            }
        }

        void SpawnBulletPattern()
        {
            if (testPattern)
            {
                foreach (Transform projectileSpawnTarget in projectileSpawnTargets)
                {
                    var spawnPosition = projectileSpawnTarget.position;
                    var direction = projectileSpawnTarget.position - transform.position;
                    var projectile = BulletManager.Instance.SpawnBullet(attackData.ammo, spawnPosition, Quaternion.LookRotation(direction));
                }
            }

            if (!testPattern)
            {
                foreach (Transform projectileDiagSpawnTarget in projectileDiagSpawnTargets)
                {
                    var spawnPosition = projectileDiagSpawnTarget.position;
                    var direction = projectileDiagSpawnTarget.position - transform.position;
                    var projectile = BulletManager.Instance.SpawnBullet(attackData.ammo, spawnPosition, Quaternion.LookRotation(direction));
                }
            }
        }
        public UnityEvent OnDamage { get; } = new();
        public UnityEvent OnDeath { get; } = new();
        public void TakeDamage(DamageData damage)
        {
            
        }

        public void PlayDamageEffect(Color colour)
        {
            
        }
    }
}
