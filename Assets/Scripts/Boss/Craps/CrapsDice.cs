using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Events;
using Interfaces;
using UnityEngine;
using Weapon;

namespace Boss.Craps
{
    /// <summary>
    /// Craps Dice. Dice that rolls into the arena and does a attack based on the face.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CrapsDice : Damageable 
    {
        /// <summary>
        /// Rigidbody of the dice.
        /// </summary>
        private Rigidbody _rigidbody;

        /// <summary>
        /// Launch force of the dice.
        /// </summary>
        [SerializeField] private float launchForce = 500f;
        
        /// <summary>
        /// Torque force of the dice.
        /// </summary>
        [SerializeField] private float torqueForce = 150f;

        /// <summary>
        /// Damage to deal on collision.
        /// </summary>
        [SerializeField] private DamageData collisionDamage;

        /// <summary>
        /// Number of projectiles to fire around the dice.
        /// </summary>
        [SerializeField] private int projectileCount = 6;
        
        /// <summary>
        /// Rate at which to fire projectiles.
        /// </summary>
        [SerializeField] private float attackRate = 1f;
        
        /// <summary>
        /// Ammo to fire.
        /// </summary>
        [SerializeField] private Ammo ammo;
        
        /// <summary>
        /// Speed at which to fly away.
        /// </summary>
        [SerializeField] private float FlyAwaySpeed = 3f;

        [SerializeField] private AudioEvent PlayDeathSoundEvent;

        [SerializeField] private AudioEvent PlayShootBulletSoundEvent;

        [SerializeField] private AudioClip AnnoyingHumSound;
        
        /// <summary>
        /// Flag to check if damage has been dealt.
        /// </summary>
        private bool _dealtDamage = false;
        
        /// <summary>
        /// Flag to check if the dice is moving.
        /// </summary>
        private bool _isMoving = true;

        /// <summary>
        /// List of dice faces.
        /// </summary>
        [SerializeField] private List<Transform> diceFaces;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            Launch();
        }

        /// <summary>
        /// Coroutine to fire projectiles at a set rate.
        /// </summary>
        /// <returns></returns>
        private IEnumerator AttackRoutine()
        {
            while (true)
            {
                Fire();
                PlayShootBulletSoundEvent?.Invoke();
                yield return new WaitForSeconds(attackRate);
            }
        }

        /// <summary>
        /// Start the movement routine.
        /// </summary>
        private void StartMovementRoutine()
        {
            StartCoroutine(MovementRoutine());
        }
        
        /// <summary>
        /// Coroutine to detect movement of the dice.
        /// </summary>
        /// <returns></returns>
        private IEnumerator MovementRoutine()
        {
            while (_isMoving)
            {
                _isMoving = DetectMovement();
                yield return new WaitForSeconds(.5f);
            }
            
            StartAttack();
        }

    
        /// <summary>
        /// Detect the current face of the dice and do the corresponding attack.
        /// </summary>
        private void StartAttack()
        {
            var currentFace = DetectDiceFace();
            switch (currentFace)
            {
                case < 3:
                    StartCoroutine(AttackRoutine());
                    break;
                case > 4:
                    StartCoroutine(RollAtPlayer());
                    break;
            }
        }

        /// <summary>
        /// Detect if the dice is moving.
        /// </summary>
        /// <returns>If the dice is moving.</returns>
        private bool DetectMovement()
        {
            return !(_rigidbody.linearVelocity.magnitude < 0.1f && _rigidbody.angularVelocity.magnitude < 0.1f);
        }

        /// <summary>
        /// Detect the up face of the dice.
        /// </summary>
        /// <returns>The value of the up facing side.</returns>
        private int DetectDiceFace()
        {
            // Get the index of the face with the highest Y value
            var highestY = diceFaces.Max(face => face.position.y);
            var result = diceFaces.FindIndex(face => face.position.y.Equals(highestY)) + 1;

            return result;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.CompareTag("PlayerBullet"))
            {
                Debug.Log("Player hit the dice");
            }
            
            else if (other.gameObject.CompareTag("Player"))
            {
                DamageTarget(other.gameObject.GetComponent<IDamageable>());
            }
        }

        /// <summary>
        /// Damage the target if it is damageable.
        /// </summary>
        /// <param name="damageable">The target to apply damage to.</param>
        private void DamageTarget(IDamageable damageable)
        {
            if (_dealtDamage || damageable == null) return;
            _dealtDamage = true;
            
            damageable.TakeDamage(collisionDamage);
            FlyAway();
        }

        /// <summary>
        /// Fly the dice away and destroy it.
        /// </summary>
        private void FlyAway()
        {
            transform.DOMoveY(transform.position.y + 100, FlyAwaySpeed).OnComplete(() =>
            {
                Destroy(gameObject);
            });
        }

        /// <summary>
        /// Fire projectiles around the dice.
        /// </summary>
        private void Fire()
        {
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
        /// Coroutine to roll at the player. Damaging the player on collision.
        /// </summary>
        /// <returns></returns>
        private IEnumerator RollAtPlayer()
        {
            var player = GameObject.FindGameObjectWithTag("Player").transform;
            // Start with a dotween shake and then roll towards the player
            var tween = transform.DOShakePosition(2f, 3f, 1, 90f, false, true)
                .OnComplete(() =>
                {
                    var direction = (player.position - transform.position).normalized;
                    _rigidbody.AddForce(direction * launchForce);
                    _rigidbody.AddTorque(direction * torqueForce);
                    
                });
            
            while (tween.IsActive())
            {
                yield return null;
            }

            // Spin while moving towards player
            transform.DORotate(new Vector3(360, 360, 360), 1f, RotateMode.FastBeyond360).SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Incremental);
            
            while (true)
            {
                // Move towards player
                var targetPosition = player.position;
                targetPosition.y = transform.position.y;
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, 5f * Time.deltaTime);
                yield return null;
            }
        }

        /// <summary>
        /// Launch the dice forward.
        /// </summary>
        private void Launch()
        {
            _isMoving = true;
            // Launch the die forward
            var randomForce = UnityEngine.Random.Range(-1, 1) * 150f;
            _rigidbody.AddForce(transform.forward * (launchForce + randomForce));

            // Add a random torque to the die
            _rigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * torqueForce);
            
            // Delay movement detection to allow for the dice to start moving
            Invoke(nameof(StartMovementRoutine), .5f);
        }

        public override void TakeDamage(DamageData damage)
        {
            if (damage.type != DamageType.Player) return;
            base.TakeDamage(damage);
        }

        protected override void Die()
        {
            base.Die();
            PlayDeathSoundEvent?.Invoke();
            FlyAway();
        }
    }
}