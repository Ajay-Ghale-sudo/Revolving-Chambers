using DG.Tweening;
using Interfaces;
using State;
using UnityEngine;
using Weapon;

namespace LevelHazards
{
    public class DiamondMirror : Damageable
    {
        
        [SerializeField]
        private Ammo _ammo;

        // Duration for the mirror to shake when hit by a boss or player projectile.
        [SerializeField] 
        private float shakeDuration = 0.5f;

        // Strength for the mirror to shake when hit by a boss or player projectile.
        [SerializeField] 
        private float shakeStrength = 0.03f;

        // Speed in seconds that the mirror should retract into the ground (lower values retract faster.)
        [SerializeField] private float retractSpeed = 0.5f;
        
        /// <summary>
        /// Vertical offset for the diamond mirror
        /// </summary>
        [SerializeField]
        private float verticalOffset = 5.5f;

        private Transform _playerTransform;
        private Transform _bossTransform;
        private CapsuleCollider _mirrorCollider;

        private float _initialYPos;
        
        private bool _broken = false;
        private bool _shootAtPlayer = true;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            _bossTransform = GameObject.FindGameObjectWithTag("Boss").transform;
            _mirrorCollider = GetComponent<CapsuleCollider>();
            _initialYPos = transform.position.y;

            GameStateManager.Instance.OnBossFightStart += EnableCollider;
            GameStateManager.Instance.OnBossDeath += HandleBossDeath;
        }

        private void EnableCollider()
        {
            _mirrorCollider.enabled = true;
        }

        private void OnDestroy()
        {
            GameStateManager.Instance.OnBossDeath -= HandleBossDeath;
        }

        private void OnTriggerEnter(Collider other)
        {
           if (other.TryGetComponent(out Ammo ammo))
           {
               _ammo = ammo;
           } 
        }

        public override void TakeDamage(DamageData damage)
        {
            if (_broken) return;
            
            switch (damage.type)
            {
                case DamageType.Player:
                    _shootAtPlayer = false;
                    DamagedByPlayer();
                    break;
                case DamageType.Boss:
                    _shootAtPlayer = true;
                    DamagedByBoss();
                    break;
            }

            _broken = true;
            Invoke(nameof(Rise), 3f);
            OnDamage?.Invoke();
        }
        
        /// <summary>
        /// Cleanup when the boss dies.
        /// </summary>
        private void HandleBossDeath()
        {
            _shootAtPlayer = false;
        }

        private void Shoot()
        {
            // Mirror stays broken if shot by player for now
            if (!_shootAtPlayer) return;
            // Default to shooting at player for now
            var target = _shootAtPlayer ? _playerTransform : _bossTransform;
            var direction = (target.position - transform.position);
            direction.y = 0;
            direction.Normalize();
            var firePos = transform.position + new Vector3(0, -1f, 0);
            var bullet = BulletManager.Instance.SpawnBullet(_ammo, firePos + direction * 2f, Quaternion.LookRotation(direction));
            //bullet.gameObject.layer = LayerMask.NameToLayer("BossBullet");

            Fix();
        }
        
        private void Rise()
        {
            transform.DOMoveY(verticalOffset, 1f).SetRelative(true).OnComplete(Shoot);
        }

        private void Fix()
        {
            _broken = false;
        }

        private void DamagedByPlayer()
        {
            transform.DOMoveY(-verticalOffset, 0.5f).SetRelative(true);
        }

        private void DamagedByBoss()
        {
            transform.DOShakePosition(shakeDuration, shakeStrength, 5, 90f, false, true).SetRelative(true)
                .OnComplete(() => transform.DOMoveY(-verticalOffset, retractSpeed).SetRelative(true));
        }
    }
}
