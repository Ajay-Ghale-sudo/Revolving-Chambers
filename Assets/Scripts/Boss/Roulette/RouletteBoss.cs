using System;
using System.Collections.Generic;
using Audio;
using Interfaces;
using Props.SlotMachine;
using State;
using UI;
using UnityEngine;
using UnityEngine.Events;
using Weapon;

namespace Boss.Roulette
{
    /// <summary>
    /// Roulette Boss.
    /// </summary>
    public class RouletteBoss : Damageable
    {
        /// <summary>
        /// Transform of the target to shoot at
        /// </summary>
        private Transform _target;
        
        /// <summary>
        /// Offset from the target to shoot at
        /// </summary>
        [SerializeField]
        private Vector3 _targetOffset = new Vector3(1f, 0f, 0f);
        
        /// <summary>
        /// State machine for the boss
        /// </summary>
        private StateMachine _stateMachine;
        
        /// <summary>
        /// Transform of the muzzle to shoot from
        /// </summary>
        [SerializeField]
        private Transform muzzleTransform;
        
        /// <summary>
        /// Default ammo to shoot at player
        /// </summary>
        [SerializeField]
        private Ammo _ammo;
        
        /// <summary>
        /// Amount of ammo to shoot in a spread shot.
        /// </summary>
        [SerializeField]
        private int _spreadShotAmount = 5;

        /// <summary>
        /// Event for when the boss shoots.
        /// </summary>
        public UnityEvent OnShotFired;
        
        /// <summary>
        /// List of environmental hazards to activate.
        /// </summary>
        [SerializeField]
        private List<GameObject> _hazards = new List<GameObject>();
        
        /// <summary>
        /// Duration of the environment hazards.
        /// </summary>
        [SerializeField]
        private float hazardDuration = 5f;
        
        /// <summary>
        /// Prefab for the following hazard to spawn
        /// </summary>
        [SerializeField]
        private GameObject _followingHazardPrefab;
        
        /// <summary>
        /// The left wheel of the slot machine
        /// </summary>
        [SerializeField]
        public SlotWheel leftWheel;
        
        /// <summary>
        /// The center wheel of the slot machine
        /// </summary>
        [SerializeField]
        public SlotWheel centerWheel;
        
        /// <summary>
        /// The right wheel of the slot machine
        /// </summary>
        [SerializeField]
        public SlotWheel rightWheel;
        
        /// <summary>
        /// Background music clip to loop.
        /// </summary>
        [SerializeField]
        private AudioClip backgroundMusic;
        
        private void Awake()
        {
            _stateMachine = new StateMachine();
            CreateStates();
        }
        
        private void Start()
        {
           UIManager.Instance.OnBossSpawned?.Invoke(name); 
           UIManager.Instance.OnBossHealthChange?.Invoke(Health / MaxHealth);
           UpdateTarget();
           
            if (backgroundMusic != null)
            {
                AudioManager.Instance?.SetBackgroundMusic(backgroundMusic, 0.6f);
            }
           
        }
        
        private void Update()
        {
            _stateMachine.Update();
            if (IsDead) return;
            LookAtTarget();
        }
        
        
        /// <summary>
        /// Create the states for the boss.
        /// </summary>
        private void CreateStates()
        {
            
            // Not used yet, but could be used for the intro.
           var idleState = new RouletteIdleState(this);
           
           var shootState = new RouletteSingleAttackState(this);
           var spreadShotState = new RouletteSpreadAttackState(this);
           var hazardState = new RouletteHazardsState(this);
           var deadState = new RouletteDeadState(this);
           var phase1 = new BossPhase();
           var phase2 = new BossPhase();
           var phase3 = new BossPhase();
           
           phase1.AddState(shootState);
           
           _stateMachine.SetState(phase1);
           
           phase2.AddState(shootState);
           phase2.AddState(hazardState);
           
           _stateMachine.AddTransition(phase1, phase2, new FuncPredicate(() => health < MaxHealth * .75f));
           
           phase3.AddState(spreadShotState);
           phase3.AddState(hazardState);
           
           _stateMachine.AddTransition(phase2, phase3, new FuncPredicate(() => health < MaxHealth * .5f));
           _stateMachine.AddAnyTransition(deadState, new FuncPredicate(() => health <= 0));
           
        }
        
        /// <summary>
        /// Rotate to face the target.
        /// </summary>
        private void LookAtTarget()
        {
            if (!_target) return;
            
            // Rotate towards the target but not up and down
            var targetPosition = new Vector3(_target.position.x, transform.position.y, _target.position.z) + _targetOffset;
            transform.LookAt(targetPosition);
        }

        /// <summary>
        /// Update the target to shoot at.
        /// </summary>
        private void UpdateTarget()
        {
            var targetObject = GameObject.FindGameObjectWithTag("Player");
            if (targetObject)
            {
                _target = targetObject.transform;
            }
            
        }

        /// <summary>
        /// Shoot at the target.
        /// </summary>
        public void ShootAtTarget()
        {
            if (!_ammo) return;
            // Shoot at the target
            var direction = _target.position - muzzleTransform.position;
            direction.Normalize();
            var rotation = Quaternion.LookRotation(direction);
            
            var bullet = BulletManager.Instance.SpawnBullet(_ammo, muzzleTransform.position + direction * 2f, rotation);   
            OnShotFired?.Invoke();
        }

        /// <summary>
        /// Shoot a spread shot at the target.
        /// </summary>
        public void ShootSpreadShot()
        {
            if (!_ammo) return;
            // shoot a spread shot, over a arc of 90 degrees
            for (int index = 0; index < _spreadShotAmount; index++)
            {
                var bulletNum = _spreadShotAmount / 2 - index;
                var dir = _target.position - transform.position;
                dir.Normalize();

                var direction = _target.position - muzzleTransform.position;
                var spawnPos = muzzleTransform.position + direction * .1f;
                spawnPos.y = muzzleTransform.position.y;
                spawnPos.x += bulletNum;
                // Angle out the farther away from the center
                var angle = 45 / _spreadShotAmount * -bulletNum;
                direction = Quaternion.AngleAxis(angle, Vector3.up) * direction;
                direction.Normalize();
                
                var bullet = BulletManager.Instance.SpawnBullet(_ammo, spawnPos, Quaternion.LookRotation(direction));                
            }
            OnShotFired?.Invoke();
        }
        
        /// <summary>
        /// Spawn a following hazard at the target.
        /// </summary>
        public void SpawnHazard()
        {
            var hazard = Instantiate(_followingHazardPrefab, _target.position, Quaternion.identity);
        }
        
        /// <summary>
        /// Activate the hazards.
        /// </summary>
        public void ActivateHazards()
        {
            foreach (var hazard in _hazards)
            {
                hazard.SetActive(true);
            }
            Invoke(nameof(DeactivateHazards), hazardDuration);
        }
        
        /// <summary>
        /// Deactivate the hazards.
        /// </summary>
        public void DeactivateHazards()
        {
            foreach (var hazard in _hazards)
            {
                hazard.SetActive(false);
            }
        }
        
        /// <summary>
        /// Remove all hazards.
        /// </summary>
        public void RemoveHazards()
        {
            foreach (var hazard in _hazards)
            {
                Destroy(hazard);
            }
            _hazards.Clear();
        }

        public override void TakeDamage(DamageData damage)
        {
            if (damage.type != DamageType.Player) return;
            health -= damage.damage;
            OnDamage?.Invoke();
            UIManager.Instance.OnBossHealthChange?.Invoke(Health / MaxHealth);
            
            if (health <= 0)
            {
                GameStateManager.Instance.lastKilledBoss = BossType.Roulette;
                Die();
            }
        }
    }
}