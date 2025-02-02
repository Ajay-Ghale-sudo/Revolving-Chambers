using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
using DG.Tweening;
using Events;
using Interfaces;
using State;
using UI;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Weapon;

namespace Boss.Craps
{
    /// <summary>
    /// Craps Table Boss. Giant dice boss that rolls around the arena.
    /// </summary>
    public class CrapsBoss : BossBase
    {
        /// <summary>
        /// State machine for the boss
        /// </summary>
        private StateMachine _stateMachine;

        /// <summary>
        /// Ammo the boss will shoot.
        /// </summary>
        [SerializeField] private Ammo spreadShotAmmo;

        /// <summary>
        /// Amount of ammo to shoot in a spread shot.
        /// </summary>
        [SerializeField] private int spreadShotAmount = 5;

        /// <summary>
        /// List of dice spawners to spawn dice from.
        /// </summary>
        [SerializeField]
        private List<DiceSpawner> diceSpawners;

        /// <summary>
        /// Background music clip to loop.
        /// </summary>
        [SerializeField]
        private AudioClip backgroundMusic;

        /// <summary>
        /// Audio event to fire on boss death.
        /// </summary>
        [SerializeField] private AudioEvent DeathAudioEvent;
        
        /// <summary>
        /// Audio event to fire when the boss takes damage.
        /// </summary>
        [SerializeField] private AudioEvent TakeDamageAudioEvent;
        
        /// <summary>
        /// Audio event to fire when the boss moves.
        /// </summary>
        [SerializeField] public AudioEvent MoveAudioEvent;

        /// <summary>
        /// Audio event to fire when the boss shoots bullets.
        /// </summary>
        [SerializeField] private AudioEvent ShootBulletAudioEvent;
        
        /// <summary>
        /// Coroutine to roll dice at random intervals.
        /// </summary>
        private Coroutine _rollDiceCoroutine;

        private void Awake()
        {
            _stateMachine = new StateMachine();
            CreateStates();
        }

        private void Start()
        {
            InitBoss();
            _rollDiceCoroutine = StartCoroutine(RollDiceCoroutine());
            if (backgroundMusic != null)
            {
                AudioManager.Instance?.SetBackgroundMusic(backgroundMusic, 0.4f);
            }
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        /// <summary>
        /// Create the states for the boss.
        /// </summary>
        private void CreateStates()
        {
            var idleState = new CrapsIdleState(this);
            var rollingState = new SideRollingState(this);

            _stateMachine.SetState(rollingState);
        }

        /// <summary>
        /// Shoot a spread shot of bullets.
        /// </summary>
        public void Shoot()
        {
            if (spreadShotAmmo == null) return;
            // shoot a spread shot, over a arc of 90 degrees
            for (int index = 0; index < spreadShotAmount; index++)
            {
                var bulletNum = spreadShotAmount / 2 - index;
                var dir = new Vector3(transform.position.x, 0, 0);
                dir.Normalize();

                var direction = -transform.forward;
                var spawnPos = transform.position + direction * 5;
                spawnPos.y = 1; // TODO: Review default bullet height
                spawnPos.x += bulletNum;
                // Angle out the farther away from the center
                var angle = 45 / spreadShotAmount * -bulletNum;
                direction = Quaternion.AngleAxis(angle, Vector3.up) * direction;
                direction.Normalize();

                var bullet = BulletManager.Instance.SpawnBullet(spreadShotAmmo, spawnPos, Quaternion.LookRotation(direction));
            }

            ShootBulletAudioEvent?.Invoke();
        }

        /// <summary>
        /// Coroutine to roll dice at random intervals.
        /// </summary>
        /// <returns></returns>
        private IEnumerator RollDiceCoroutine()
        {
            while (true)
            {
                RollDice();
                var cooldown = UnityEngine.Random.Range(5f, 10f);
                yield return new WaitForSeconds(cooldown);
            }
        }

        /// <summary>
        /// Roll a dice at a random dice spawner.
        /// </summary>
        public void RollDice()
        {
            // Pick a random dice spawner and trigger it
            var randomIndex = UnityEngine.Random.Range(0, diceSpawners.Count);
            diceSpawners[randomIndex].SpawnDice();
        }

        /// <summary>
        /// Process incoming damage.
        /// </summary>
        /// <param name="damage">The incoming damage.</param>
        public override void TakeDamage(DamageData damage)
        {
            if (damage.type != DamageType.Player) return;
            base.TakeDamage(damage);
            UIManager.Instance.OnBossHealthChange?.Invoke(health / maxHealth);
            TakeDamageAudioEvent?.Invoke();
        }

        /// <summary>
        /// Cleanup when the boss dies.
        /// </summary>
        protected override void Die()
        {
            base.Die();
            GameStateManager.Instance.lastKilledBoss = BossType.Craps;
            _stateMachine.SetState(new CrapsDeadState(this));
            if (_rollDiceCoroutine != null) StopCoroutine(_rollDiceCoroutine);
            _rollDiceCoroutine = null;
            transform.DOShakePosition(2f, 2.5f, 10, 90f, false, true).OnComplete(OnDeathFinished);
            transform.DOShakeScale(2f, 2.5f, 10, 90f);
            NextPhase();
            DeathAudioEvent?.Invoke();
        }

        protected void OnDeathFinished()
        {
            GameStateManager.Instance.OnBossDeath?.Invoke();
            gameObject.SetActive(false);
        }
    }
}