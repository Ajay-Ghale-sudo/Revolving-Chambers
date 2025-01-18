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
using Object = UnityEngine.Object;
using StateMachine = State.StateMachine;

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
        internal float _splineLength = 0f;
        
        /// <summary>
        /// The movement speed of the boss.
        /// </summary>
        public float moveSpeed = 1f;
        
        /// <summary>
        /// The current distance along the spline.
        /// </summary>
        internal float _currentDistance = 0f;

        
        /// <summary>
        /// Time between boss attacks.
        /// </summary>
        [SerializeField] internal float attackInterval = 2f;

        /// <summary>
        /// The rotation vector for the boss.
        /// </summary>
        [SerializeField] internal Vector3 rotationVector = new Vector3(0f, 360f, 0f);

        /// <summary>
        /// The duration of the rotation. Shorter duration, faster spin.
        /// </summary>
        [SerializeField] internal float rotationDuration = 1f;
        
        /// <summary>
        /// The mode of rotation for the boss.
        /// </summary>
        [SerializeField] internal RotateMode rotateMode = RotateMode.FastBeyond360;

        /// <summary>
        /// The state machine for the boss.
        /// </summary>
        StateMachine _stateMachine;
        
        /// <summary>
        /// Coroutine for the attack pattern.
        /// </summary>
        Coroutine _attackPatternCoroutine;

        private void Awake()
        {
            _stateMachine = new StateMachine();
            
            var idleState = new DefaultAttackState(this);
            _stateMachine.SetState(idleState);
            
            var deathState = new DeadState(this);
            deathState._onDeathFinished = Die;
            _stateMachine.AddTransition(idleState, deathState, new FuncPredicate(() => health <= 0));
            
            // Example of what a transition would look like.
            // var attack2 = new AttackState2(this);
            // _stateMachine.AddTransition(idleState, attack2, new FuncPredicate(() => health < maxHealth * 0.7f));
            
        }

        void Start()
        {
            UIManager.Instance.OnBossSpawned?.Invoke(name);
            UIManager.Instance.OnBossHealthChange?.Invoke(health / maxHealth);

        }
        void Update()
        {
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
           _stateMachine.FixedUpdate(); 
        }

        /// <summary>
        /// Coroutine for the attack pattern.
        /// </summary>
        /// <returns></returns>
        private IEnumerator AttackPatternCoroutine()
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

        public void StartAttackPattern()
        {
            _attackPatternCoroutine = StartCoroutine(AttackPatternCoroutine());
        }

        public void StopAttackPattern()
        {
            if (_attackPatternCoroutine != null)
            {
                StopCoroutine(_attackPatternCoroutine);
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
        }

        protected sealed override void Die()
        {
            _running = false;
            OnDeath?.Invoke();
            GameStateManager.Instance.OnBossDeath?.Invoke();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// State for the default attack pattern.
    /// </summary>
    class DefaultAttackState : BaseState<DiamondBoss>
    {
        public DefaultAttackState(DiamondBoss owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            _owner.transform.DORotate(_owner.rotationVector, _owner.rotationDuration, _owner.rotateMode)
                .SetLoops(-1).SetRelative(true).SetEase(Ease.Linear);

            _owner._splineLength = _owner.spline.CalculateLength();
            
            _owner.StartAttackPattern();
        }

        public override void OnExit()
        {
            _owner.StopAttackPattern();
            _owner.transform.DOPause();
        }

        public override void Update()
        {
            // Calculate the target position on the spline.
            // Sets the world position for the player to move to calculated by the normalized value currentDistance.
            Vector3 targetPosition = _owner.spline.EvaluatePosition(_owner._currentDistance);
            
            // Move the character towards the target position on the spline.
            _owner.transform.position = Vector3.MoveTowards(_owner.transform.position, targetPosition, _owner.moveSpeed * Time.deltaTime);
            
            // calculate how far along we are 0 -> 1.0
            _owner._currentDistance = (_owner._currentDistance + ((_owner.moveSpeed * Time.deltaTime) / _owner._splineLength)) % 1f;
        }
    }
    
    /// <summary>
    /// State for when the boss is dead.
    /// </summary>
    class DeadState : BaseState<DiamondBoss>
    {
        /// <summary>
        /// Event invoked when the death animation is finished.
        /// </summary>
        internal Action _onDeathFinished;
        
        public DeadState(DiamondBoss owner) : base(owner)
        {
        }
        
        /// <summary>
        /// Invokes the death finished event.
        /// </summary>
        private void OnDeathFinished()
        {
            _onDeathFinished?.Invoke();
        }

        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public override void OnEnter()
        {
            _owner.transform.DOShakePosition(2f, 2.5f, 10, 90f, false, true).OnComplete(OnDeathFinished);
            _owner.transform.DOShakeScale(2f, 2.5f, 10, 90f);
        }
    }
}
