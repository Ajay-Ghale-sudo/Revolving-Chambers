using System;
using DG.Tweening;
using Interfaces;
using Props;
using UnityEngine;
using Weapon;

namespace LevelHazards
{
    public class DiamondMirror : Damageable
    {
        
        [SerializeField]
        private Ammo _ammo;
        
        /// <summary>
        /// Vertical offset for the diamond mirror
        /// </summary>
        [SerializeField]
        private float verticalOffset = 5.5f;

        private Transform _playerTransform;
        private Transform _bossTransform;

        private float _initialYPos;
        
        private bool _broken = false;
        private bool _shootAtPlayer = true;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            _bossTransform = GameObject.FindGameObjectWithTag("Boss").transform;
            _initialYPos = transform.position.y;
            // transform.DORotate(new Vector3(0f, -360.0f, 0.0f), 30f, RotateMode.FastBeyond360).SetLoops(-1).SetRelative(true).SetEase(Ease.Linear);
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

        private void Shoot()
        {
            // Mirror stays broken if shot by player for now
            if (!_shootAtPlayer) return;
            // Default to shooting at player for now
            var target = _shootAtPlayer ? _playerTransform : _bossTransform;
            var direction = (target.position - transform.position).normalized;
            BulletManager.Instance.SpawnBullet(_ammo, transform.position + direction * 2f, Quaternion.LookRotation(direction));

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
            transform.DOShakePosition(1f, .01f, 1, 90f, false, true).SetRelative(true)
                .OnComplete(() => transform.DOMoveY(-verticalOffset, 1f).SetRelative(true));
        }
    }
}
