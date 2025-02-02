using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using Boss.Craps;
using DG.Tweening;
using Events;
using Interfaces;
using State;
using UI;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;
using Weapon;
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
    public class DiamondBoss : BossBase 
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
        /// The audio used for the boss' idle sounds before the fight.
        /// </summary>
        [SerializeField]
        [Tooltip("The audio used for the boss' idle sounds before the fight.")]
        public AudioClip _idleSound;

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

            var phase1State = new Phase1State(this);
            _stateMachine.AddTransition(bossIntroState, phase1State, new FuncPredicate(() => bossIntroState.IsComplete));
            
            var phase2State = new Phase2State(this);
            _stateMachine.AddTransition(phase1State, phase2State, new FuncPredicate(() => health <= 0.0f));

            var phase3State = new Phase3State(this);
            _stateMachine.AddTransition(phase2State, phase3State, new FuncPredicate(() => health <= 0.0f));

            var deathState = new DeadState(this);
            deathState._onDeathFinished = Die;
            _stateMachine.AddTransition(phase3State, deathState, new FuncPredicate(() => health <= 0.0f));
        }

        void Start()
        {
            InitBoss();
        }
        void Update()
        {
            _stateMachine.Update();
        }

        private void FixedUpdate()
        {
           _stateMachine.FixedUpdate(); 
        }

        public void HealToFull()
        {
            health = maxHealth;
            UIManager.Instance.OnBossHealthChange?.Invoke(health / maxHealth);
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
                projectile.gameObject.layer = LayerMask.NameToLayer("BossBullet");
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
            GameStateManager.Instance.lastKilledBoss = BossType.Diamond;
            AudioManager.Instance?.SetBackgroundMusic(null, 0.0f);
            _running = false;
            OnDeath?.Invoke();
            GameStateManager.Instance?.OnBossDeath?.Invoke();
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

        /// <summary>
        /// Fires a single bullet from a specified position in a specified direction.
        /// </summary>
        /// <param name="worldPosition">World coordinates that the bullet should start from.</param>
        /// <param name="worldDirection">Normalized vector that the bullet should travel along.</param>
        public void FireBullet(float3 worldPosition, Vector3 worldDirection)
        {
            // Spawn bullet with provided parameters
            var bullet = BulletManager.Instance.SpawnBullet(attackData.ammo, worldPosition, Quaternion.LookRotation(worldDirection));
            
            // Ensure the bullet can't collide with the boss
            bullet.gameObject.layer = LayerMask.NameToLayer("BossBullet");
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
            AudioManager.Instance.SetBackgroundMusic(_owner._idleSound, 0.5f);
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

            AudioManager.Instance?.SetBackgroundMusic(_owner._bossIntroMusic, 0.4f);
            GameStateManager.Instance?.OnBossIntroStart?.Invoke();
        }

        public override void OnExit()
        {
            AudioManager.Instance?.SetBackgroundMusic(_owner._backgroundMusic, 0.4f);
            GameStateManager.Instance?.OnBossFightStart?.Invoke();
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
        private float tStart = 0.0f;
        private float tTraveled = 0.0f;
        private float tPrevFinish = 0.0f;
        private const float MoveAmount = 0.25f;

        private float travelDirection = 1.0f;
        private float prevTravelDirection = -1.0f;

        private int numBullets = 0;
        private float tBulletInterval = MoveAmount;

        public bool IsComplete = false;
        public DefaultMoveState(DiamondBoss owner, int numBulletsToShoot = 0) : base(owner)
        {
            numBullets = numBulletsToShoot;
            tBulletInterval = MoveAmount / (numBullets + 1);
        }

        /// <summary>
        /// Get a random direction to travel in.
        /// </summary>
        /// <returns>Either -1 or 1</returns>
        float getRandomDirection()
        {
            return UnityEngine.Random.Range(0, 1) * 2 - 1;
        }

        /// <summary>
        /// Get a random segment position to start at.
        /// </summary>
        /// <returns>A value corresponding to the boundary between segments (a multiple of MoveAmount)</returns>
        float getRandomSegmentPosition()
        {
            const int numSegments = (int)(1.0f / MoveAmount);
            return UnityEngine.Random.Range(0, numSegments) / (float)numSegments;
        }

        /// <summary>
        /// Logic to run when the state is entered initially.
        /// </summary>
        public override void OnEnter()
        {
            IsComplete = false;

            // If above half health, move continuously (ie. don't teleport)
            if (_owner.Health >= _owner.MaxHealth / 2.0f)
            {
                tStart = tPrevFinish;
                travelDirection = 1.0f;
            }
            // If lower than half health, force a teleport in a different segment each cycle
            else
            {
                // Re-roll start position until it starts in a new segment
                do tStart = getRandomSegmentPosition(); while ((Math.Abs(tStart - tPrevFinish) < 0.01f));
                
                // Also, if new move action will place the boss in the same place, move it in the other direction
                travelDirection = getRandomDirection();
                var wrappedFinishPosition = Mathf.Repeat(tStart + MoveAmount * travelDirection, 1.0f);
                var deltaWithPrevFinish = Math.Abs(wrappedFinishPosition - tPrevFinish);
                if (deltaWithPrevFinish < 0.01f)
                {
                    travelDirection *= -1;
                }
            }
            
            tTraveled = 0.0f;
            
            _owner.transform.position = _owner.spline.EvaluatePosition(tStart);
            // _owner._trailRenderer.emitting = false;
            _owner.transform.DORotate(_owner.rotationVector, _owner.rotationDuration, _owner.rotateMode)
                .SetLoops(-1).SetRelative(true).SetEase(Ease.Linear);

            _owner._splineLength = _owner.spline.CalculateLength();
        }

        /// <summary>
        /// Logic to run when the state is exiting.
        /// </summary>
        public override void OnExit()
        {
            tPrevFinish = Mathf.Repeat(tStart + tTraveled * travelDirection, 1.0f);
            _owner.transform.DOPause();
        }

        /// <summary>
        /// Logic to run on each game update.
        /// </summary>
        public override void Update()
        {
            if (IsComplete) return;
            
            // Determine move speed based of remaining health
            var remainingHealth = _owner.Health / _owner.MaxHealth;
            var moveSpeed = Mathf.Lerp(_owner.moveSpeed * 1.5f, _owner.moveSpeed, remainingHealth);

            // Determine how far we've moved on [0.0f, MoveAmount] along the spline
            var tTraveledPrev = tTraveled;
            tTraveled = Math.Clamp(tTraveled + moveSpeed * Time.deltaTime, 0.0f, MoveAmount);
            
            // Add that to the origin on the spline and wrap it to [0.0f, 1.0f]
            var tAlongSpline = Mathf.Repeat(tStart + tTraveled * travelDirection, 1.0f);
            
            // Update boss position with the resulting world coordinates
            _owner.transform.position = _owner.spline.EvaluatePosition(tAlongSpline);
            
            // If there are any bullets to fire along this path, try to fire them
            if (numBullets > 0)
            {
                var numBulletsFiredSoFar = (int)(tTraveledPrev / tBulletInterval);
                var numBulletsFiredAfterThisUpdate = (int)(tTraveled / tBulletInterval);
                for (var bulletIndex = numBulletsFiredSoFar; bulletIndex < numBulletsFiredAfterThisUpdate; bulletIndex++)
                {
                    // Bullet should be evenly-spaced along the spline, but at the height of the boss
                    var positionAlongSpline = tStart + bulletIndex * tBulletInterval * travelDirection;
                    var wrappedBulletPos = Mathf.Repeat(positionAlongSpline, 1.0f);
                    var bulletWorldPosition = _owner.spline.EvaluatePosition(wrappedBulletPos);
                    bulletWorldPosition.y = 2.0f;
                    
                    // Bullet should travel along the xz-plane
                    var bulletDirection = _owner.transform.position;
                    bulletDirection.y = 0.0f;
                    bulletDirection.Normalize();
                    
                    // Spawn bullet meeting these parameters
                    _owner.FireBullet(bulletWorldPosition, bulletDirection);
                    _owner.FireBullet(bulletWorldPosition, bulletDirection * -1.0f);
                }
            }
            
            if (tTraveled >= MoveAmount)
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

        private float inputPhaseTime = 0.0f;

        private float phaseEndTime = 0;
        
        /// <summary>
        /// Cooldown for the phase.
        /// </summary>
        private float phaseCooldown = 10f;
        
        public bool IsReady => Time.time - phaseEndTime > phaseCooldown;
        
        private Vector3 _startingPosition;
        
        public CenterAttackState(DiamondBoss owner, float newPhaseTime = 0.0f) : base(owner)
        {
            inputPhaseTime = newPhaseTime;
        }

        public override void OnEnter()
        {
            PhaseComplete = false;
            startTime = Time.time;
            _owner._trailRenderer.emitting = true;
            _startingPosition = _owner.transform.position;
            
            // Teleport to center of arena
            _owner.transform.position = new Vector3(0f, -0.6f, 0f);
            _owner.transform.DORotate(_owner.rotationVector, _owner.rotationDuration / 3, _owner.rotateMode)
                .SetLoops(-1).SetRelative(true).SetEase(Ease.Linear);
            
            _owner.StopAttackPattern();
            _owner.StartSecondaryAttackPattern();
            
            // Random time to complete phase
            phaseTime = inputPhaseTime <= 0.0f ? UnityEngine.Random.Range(5f, 10f) : inputPhaseTime;
        }
        
        public override void OnExit()
        {
            phaseTime = 0.0f;
            phaseEndTime = Time.time;
            _owner.transform.position = _startingPosition;
            _owner.StopAttackPattern();
            
            _owner.Invoke(nameof(DiamondBoss.DisableTrail), 1f);
        }

        public override void Update()
        {
            if (PhaseComplete) return;
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

    class Phase1State : BaseState<DiamondBoss>
    {
        private StateMachine _stateMachine;
        
        public Phase1State(DiamondBoss owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            _owner.HealToFull();
            
            _stateMachine = new StateMachine();
            CreateStates();
        }

        /// <summary>
        /// Init the states that make up phase 1 of the boss behavior.
        /// </summary>
        private void CreateStates()
        {
            var defaultMoveState = new DefaultMoveState(_owner);
            _stateMachine.SetState(defaultMoveState);
            
            var defaultAttackState = new DefaultAttackState(_owner);
            _stateMachine.AddTransition(defaultMoveState, defaultAttackState, new FuncPredicate(() => defaultAttackState.IsReady && defaultMoveState.IsComplete));
            _stateMachine.AddTransition(defaultAttackState, defaultMoveState, new FuncPredicate(() => defaultAttackState.IsComplete)); 
        }

        public override void OnExit()
        {
            GameStateManager.Instance.OnDiamondBossPhase1End?.Invoke();
        }

        /// <summary>
        /// On update, pass event through to nested state machine.
        /// </summary>
        public override void Update()
        {
            _stateMachine.Update();
        }

        /// <summary>
        /// On fixed update, pass event through to nested state machine.
        /// </summary>
        public override void FixedUpdate()
        {
            _stateMachine.FixedUpdate(); 
        }
    }

    class Phase2State : BaseState<DiamondBoss>
    {
        private StateMachine _stateMachine;
        
        public Phase2State(DiamondBoss owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            _owner.HealToFull();
            _owner.NextPhase();
            
            _stateMachine = new StateMachine();
            CreateStates();
        }

        /// <summary>
        /// Init the states that make up phase 2 of the boss behavior.
        /// </summary>
        private void CreateStates()
        {
            var centerAttackState = new CenterAttackState(_owner, 3.0f);
            _stateMachine.SetState(centerAttackState);
            
            var defaultMoveState = new DefaultMoveState(_owner, 24);
            _stateMachine.AddTransition(centerAttackState, defaultMoveState, new FuncPredicate(() => centerAttackState.PhaseComplete));
            _stateMachine.AddTransition(defaultMoveState, centerAttackState, new FuncPredicate(() => defaultMoveState.IsComplete));
        }

        public override void OnExit()
        {
            GameStateManager.Instance.OnDiamondBossPhase2End?.Invoke();
        }

        /// <summary>
        /// On update, pass event through to nested state machine.
        /// </summary>
        public override void Update()
        {
            _stateMachine.Update();
        }

        /// <summary>
        /// On fixed update, pass event through to nested state machine.
        /// </summary>
        public override void FixedUpdate()
        {
            _stateMachine.FixedUpdate(); 
        }
    }

    class Phase3State : BaseState<DiamondBoss>
    {
        private StateMachine _stateMachine;

        public Phase3State(DiamondBoss owner) : base(owner)
        {
        }

        public override void OnEnter()
        {
            _owner.HealToFull();
            _owner.NextPhase();
            
            _stateMachine = new StateMachine();
            CreateStates();
        }

        /// <summary>
        /// Init the states that make up phase 3 of the boss behavior.
        /// </summary>
        private void CreateStates()
        {
            var finalAttackState = new FinalAttackState(_owner);
            _stateMachine.SetState(finalAttackState);
        }

        public override void OnExit()
        {
        }

        /// <summary>
        /// On update, pass event through to nested state machine.
        /// </summary>
        public override void Update()
        {
            _stateMachine.Update();
        }

        /// <summary>
        /// On fixed update, pass event through to nested state machine.
        /// </summary>
        public override void FixedUpdate()
        {
            _stateMachine.FixedUpdate(); 
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
            _owner.NextPhase();
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

            _owner.transform.position = new Vector3(0f, -0.6f, 0f);
            _owner.transform.DORotate(_owner.rotationVector, _owner.rotationDuration / 3, _owner.rotateMode)
                .SetLoops(-1).SetRelative(true).SetEase(Ease.Linear);
            
            _owner.StartAttackPattern(3);
            _owner.StartSecondaryAttackPattern();
        }
    }
}
