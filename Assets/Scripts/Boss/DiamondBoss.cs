using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using DG.Tweening;
using Events;
using Interfaces;
using State;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
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
        /// Time between secondary attacks.
        /// </summary>
        [SerializeField] float secondaryAttackInterval = 4f;

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
        private StateMachine _stateMachine;
        
        /// <summary>
        /// Coroutine for the attack pattern.
        /// </summary>
        private Coroutine _attackPatternCoroutine;
        
        /// <summary>
        /// The trail renderer for the boss.
        /// </summary>
        internal TrailRenderer _trailRenderer;
        
        /// <summary>
        /// The player transform.
        /// </summary>
        private Transform _playerTransform;
        
        /// <summary>
        /// The diamond hazard for the boss.
        /// </summary>
        [SerializeField]
        [Tooltip("The diamond hazard for the boss.")]
        private GameObject _diamondHazard;

        /// <summary>
        /// The audio used for the boss intro music.
        /// </summary>
        [SerializeField]
        [Tooltip("The music played during the boss intro.")]
        public AudioClip _bossIntroMusic;

        /// <summary>
        /// The audio used for the boss' triggering background music.
        /// </summary>
        [SerializeField]
        [Tooltip("The background music for the boss.")]
        public AudioClip _backgroundMusic;

        /// <summary>
        /// The audio event played when the boss fires a bullet.
        /// </summary>
        [FormerlySerializedAs("_fireBulletEvent")]
        [SerializeField]
        [Tooltip("The audio event played when the boss fires a bullet.")]
        public AudioEvent _fireBulletAudioEvent;

        /// <summary>
        /// The object corresponding to the exit portal spawned when the boss dies.
        /// </summary>
        [SerializeField]
        [Tooltip("The exit portal spawned when the boss dies.")]
        public GameObject _bossExitPortal;

        private void Awake()
        {
            
            _trailRenderer = GetComponent<TrailRenderer>();
            
            _stateMachine = new StateMachine();
            _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            
            CreateStates();
        }
        
        /// <summary>
        /// Creates the states for the boss.
        /// </summary>
        private void CreateStates()
        {
            var bossIdleState = new BossIdleState(this);
            _stateMachine.SetState(bossIdleState);

            var bossIntroState = new BossIntroState(this);
            _stateMachine.AddTransition(bossIdleState, bossIntroState, new FuncPredicate(() => bossIdleState.IsComplete));

            var defaultMoveState = new DefaultMoveState(this);
            _stateMachine.AddTransition(bossIntroState, defaultMoveState, new FuncPredicate(() => bossIntroState.IsComplete));

            var defaultAttackState = new DefaultAttackState(this);
            _stateMachine.AddTransition(defaultMoveState, defaultAttackState, new FuncPredicate(() => defaultAttackState.IsReady && defaultMoveState.IsComplete));
            _stateMachine.AddTransition(defaultAttackState, defaultMoveState, new FuncPredicate(() => defaultAttackState.IsComplete)); 

            var centerAttackState = new CenterAttackState(this);
            _stateMachine.AddTransition(defaultMoveState, centerAttackState, new FuncPredicate(() => health < maxHealth * 0.7f && centerAttackState.IsReady));

            _stateMachine.AddTransition(centerAttackState, defaultMoveState, new FuncPredicate(() => centerAttackState.PhaseComplete));
            
            var deathState = new DeadState(this);
            deathState._onDeathFinished = Die;
            _stateMachine.AddAnyTransition(deathState, new FuncPredicate(() => health <= 0));
            
            var finalAttackState = new FinalAttackState(this);
            _stateMachine.AddAnyTransition(finalAttackState, new FuncPredicate(() => health < maxHealth * 0.3f));
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
        private IEnumerator AttackPatternCoroutine(int attackCount = 1)
        {
            _running = true;
            while (_running)
            {
                SpawnBulletPattern();
                _useAltAttackPattern = !_useAltAttackPattern;
                for (var index = 0; index < attackCount; index++)
                {
                    Invoke(nameof(SpawnBulletPattern), .5f + index);
                }

                yield return new WaitForSeconds(attackInterval);
            }
        }
        
        /// <summary>
        /// Coroutine for the secondary attack pattern.
        /// </summary>
        /// <returns></returns>
        private IEnumerator SecondaryAttackPatternCoroutine()
        {
            _running = true;
            while (_running)
            {
                SpawnHazard();
                yield return new WaitForSeconds(secondaryAttackInterval);
            }
        }

        /// <summary>
        /// Starts the attack pattern for the boss.
        /// </summary>
        /// <param name="attackCount">Amount of projectiles to spawn</param>
        public void StartAttackPattern(int attackCount = 1)
        {
            _attackPatternCoroutine = StartCoroutine(AttackPatternCoroutine(attackCount));
        }
        
        /// <summary>
        /// Starts the secondary attack pattern for the boss.
        /// </summary>
        public void StartSecondaryAttackPattern()
        {
            _attackPatternCoroutine = StartCoroutine(SecondaryAttackPatternCoroutine());
        }

        /// <summary>
        /// Stops the attack pattern for the boss.
        /// </summary>
        public void StopAttackPattern()
        {
            if (_attackPatternCoroutine != null)
            {
                StopCoroutine(_attackPatternCoroutine);
                _attackPatternCoroutine = null;
            }
        }


        /// <summary>
        /// Spawns the diamond hazard for the boss.
        /// </summary>
        private void SpawnHazard()
        {
            // Spawn a hazard that follows the player before activating.
            var hazard = Instantiate(_diamondHazard, _playerTransform.position, Quaternion.identity);
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
                direction.y = 0;
                direction.Normalize();
                var projectile = BulletManager.Instance.SpawnBullet(attackData.ammo, spawnPosition, Quaternion.LookRotation(direction));
            }
            _fireBulletAudioEvent.Invoke();
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
            if (_bossExitPortal != null) _bossExitPortal.SetActive(true);
            AudioManager.Instance.SetAmbientClip(null, 0.0f);
            _running = false;
            OnDeath?.Invoke();
            GameStateManager.Instance.OnBossDeath?.Invoke();
            Destroy(gameObject);
        }

        internal void EnableTrail()
        {
            _trailRenderer.emitting = true;
        }
        
        internal void DisableTrail()
        {
            _trailRenderer.emitting = false;
        }
    }

    /// <summary>
    /// State for boss idling before the fight.
    /// </summary>
    class BossIdleState : BaseState<DiamondBoss>
    {
        public bool IsComplete = false;
        public BossIdleState(DiamondBoss owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            IsComplete = false;
            _owner.OnDamage.AddListener(OnDamageTaken);
            AudioManager.Instance.SetAmbientClip(_owner._backgroundMusic, 0.1f);
        }

        public override void OnExit()
        {
            _owner.OnDamage.RemoveListener(OnDamageTaken);
        }

        public override void Update()
        {
        }

        private void OnDamageTaken()
        {
            IsComplete = true;
        }
    }

    /// <summary>
    /// State for boss intro.
    /// </summary>
    class BossIntroState : BaseState<DiamondBoss>
    {
        // exactly the length of the boss intro music (four bars at 170bpm)
        private const float DurationSeconds = 5.647f;
        private float elapsedTimeSeconds;
        
        public bool IsComplete = false;

        public BossIntroState(DiamondBoss owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            elapsedTimeSeconds = 0f;
            IsComplete = false;

            AudioManager.Instance.SetAmbientClip(_owner._bossIntroMusic, 0.3f);
        }

        public override void OnExit()
        {
            AudioManager.Instance.SetAmbientClip(_owner._backgroundMusic, 0.3f);
        }

        public override void Update()
        {
            if (IsComplete) return;
            
            elapsedTimeSeconds += Time.deltaTime;
            if (elapsedTimeSeconds >= DurationSeconds)
            {
                IsComplete = true;
            }
        }
    }
    
    
    /// <summary>
    /// State for the default attack pattern.
    /// </summary>
    class DefaultMoveState : BaseState<DiamondBoss>
    {
        private float startingDistance = 0f;
        private float moveAmount = 0.25f;

        public bool IsComplete = false;
        public DefaultMoveState(DiamondBoss owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            IsComplete = false;
            startingDistance = _owner._currentDistance;
            // _owner._trailRenderer.emitting = false;
            _owner.transform.DORotate(_owner.rotationVector, _owner.rotationDuration, _owner.rotateMode)
                .SetLoops(-1).SetRelative(true).SetEase(Ease.Linear);

            _owner._splineLength = _owner.spline.CalculateLength();
        }

        public override void OnExit()
        {
            _owner.transform.DOPause();
        }

        public override void Update()
        {
            if (IsComplete) return;
            // Calculate the target position on the spline.
            // Sets the world position for the player to move to calculated by the normalized value currentDistance.
            Vector3 targetPosition = _owner.spline.EvaluatePosition(_owner._currentDistance);
            
            // Determine move speed based of remaining health
            var remainingHealth = _owner.Health / _owner.MaxHealth;
            var moveSpeed = Mathf.Lerp(_owner.moveSpeed * 5f, _owner.moveSpeed, remainingHealth);
            
            // Move the character towards the target position on the spline.
            _owner.transform.position = Vector3.MoveTowards(_owner.transform.position, targetPosition, moveSpeed * Time.deltaTime);
            
            
            // calculate how far along we are 0 -> 1.0
            _owner._currentDistance = (_owner._currentDistance + ((moveSpeed * Time.deltaTime) / _owner._splineLength)) % 1f;
            
            if (_owner._currentDistance - startingDistance >= moveAmount || _owner._currentDistance < startingDistance)
            {
                IsComplete = true;
            }
            
        }
        
    }


    class DefaultAttackState : BaseState<DiamondBoss>
    {
        private float lastActiveTime = 0;
        private float cooldown = 2f;
        public bool IsReady => Time.time - lastActiveTime > cooldown;
        public float duration = 2f;
        private float elapsedTime = 0;

        public bool IsComplete = false;
        public DefaultAttackState(DiamondBoss owner) : base(owner)
        {
        }

        public override void Update()
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= duration)
            {
                Complete();
            }
        }

        private void Complete()
        {
            IsComplete = true;
        }

        public override void OnEnter()
        {
            _owner.StartAttackPattern();
            elapsedTime = 0;
            IsComplete = false;
            
            // Adjust duration based on health
            duration = Mathf.Lerp(2f, .5f, _owner.Health / _owner.MaxHealth);
            
            _owner.Invoke(nameof(DiamondBoss.DisableTrail), 1f);
            
        }
        
        public override void OnExit()
        {
            // Adjust cooldown to health
            cooldown = Mathf.Lerp(2f, .5f, _owner.Health / _owner.MaxHealth);
            
            lastActiveTime = Time.time;
            _owner.StopAttackPattern();
        }
    }

    /// <summary>
    /// State for the center attack pattern.
    /// </summary>
    class CenterAttackState : BaseState<DiamondBoss>
    {
        /// <summary>
        /// Whether the phase is complete.
        /// </summary>
        public bool PhaseComplete { get; private set; } = false;

        /// <summary>
        /// Time the phase started.
        /// </summary>
        private float startTime = 0;
        
        /// <summary>
        /// Current time in the phase.
        /// </summary>
        private float currentTime = 0;

        /// <summary>
        /// Time the phase will last.
        /// </summary>
        private float phaseTime;

        private float phaseEndTime = 0;
        
        /// <summary>
        /// Cooldown for the phase.
        /// </summary>
        private float phaseCooldown = 10f;
        
        public bool IsReady => Time.time - phaseEndTime > phaseCooldown;
        
        private Vector3 _startingPosition;
        
        public CenterAttackState(DiamondBoss owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            PhaseComplete = false;
            startTime = Time.time;
            _owner._trailRenderer.emitting = true;
            _startingPosition = _owner.transform.position;
            
            // Teleport to center of arena
            _owner.transform.position = Vector3.zero + new Vector3(0, 1, 0);
            _owner.transform.DORotate(_owner.rotationVector, _owner.rotationDuration / 3, _owner.rotateMode)
                .SetLoops(-1).SetRelative(true).SetEase(Ease.Linear);
            
            _owner.StopAttackPattern();
            _owner.StartSecondaryAttackPattern();
            
            // Random time to complete phase
            phaseTime = UnityEngine.Random.Range(5f, 10f);
        }
        
        public override void OnExit()
        {
            phaseEndTime = Time.time;
            _owner.transform.position = _startingPosition;
            _owner.StopAttackPattern();
            
            _owner.Invoke(nameof(DiamondBoss.DisableTrail), 1f);
        }

        public override void Update()
        {
            currentTime = Time.time - startTime;
            if (currentTime >= phaseTime)
            {
                CompletePhase();
            }
        }
        private void CompletePhase()
        {
            PhaseComplete = true;
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
            _owner._trailRenderer.emitting = false;
            _owner.transform.DOShakePosition(2f, 2.5f, 10, 90f, false, true).OnComplete(OnDeathFinished);
            _owner.transform.DOShakeScale(2f, 2.5f, 10, 90f);
        }
    }
    
    /// <summary>
    /// State for the Diamond Boss final attack.
    /// </summary>
    class FinalAttackState : BaseState<DiamondBoss>
    {
        public FinalAttackState(DiamondBoss owner) : base(owner)
        {
        }
        
        public override void OnEnter()
        {
            _owner._trailRenderer.emitting = true;
            
            _owner.transform.position = Vector3.zero + new Vector3(0, 1, 0);
            _owner.transform.DORotate(_owner.rotationVector, _owner.rotationDuration / 3, _owner.rotateMode)
                .SetLoops(-1).SetRelative(true).SetEase(Ease.Linear);
            
            _owner.StartAttackPattern(3);
            _owner.StartSecondaryAttackPattern();
        }
    }
}
