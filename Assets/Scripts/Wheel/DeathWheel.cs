using System;
using State;
using UI;
using UnityEngine;

namespace Wheel 
{

    /// <summary>
    /// A section on the death wheel.
    /// </summary>
    [Serializable]
    public class DeathWheelSection : WheelSection
    {
        /// <summary>
        /// Whether this section revives the player.
        /// </summary>
        public bool isRevive = false;
        
        /// <summary>
        /// Color of the section.
        /// </summary>
        public Color deathColor;
        
        /// <summary>
        /// Color of the section.
        /// </summary>
        public override Color SectionColor => deathColor;
    }
    
    /// <summary>
    /// A reward wheel to gain a 2nd wind. Used when a player dies.
    /// </summary>
    public class DeathWheel : RewardWheel<DeathWheelSection>
    {
        
        /// <summary>
        /// Cooldown in seconds for the wheel to be used again.
        /// </summary>
        [SerializeField] private float cooldown = 30f;
        
        /// <summary>
        /// Whether the wheel is on cooldown.
        /// </summary>
        private bool _onCooldown = false;
        
        /// <summary>
        /// Number of loops before a game over.
        /// </summary>
        [SerializeField]
        [Tooltip("Number of loops before a game over.")]
        private int MaxLoops = 3;
        
        void Start()
        {
            GameStateManager.Instance.OnPlayerDeath += SetupWheel;
        }
        protected override void SetupWheel()
        {
            if (_onCooldown)
            {
                GameStateManager.Instance.OnGameOver?.Invoke();
                return;
            }
            
            _onCooldown = true;
            UIManager.Instance.OnDeathWheelStart?.Invoke();

            base.SetupWheel();
            
            StopWheel();
            SpinWheel();
            
            UIManager.Instance.OnSpinEnd += StopWheel;
            
            Invoke(nameof(EnableDeathWheel), cooldown);
        }

        /// <summary>
        /// Enable the death wheel after the cooldown.
        /// </summary>
        private void EnableDeathWheel()
        {
            _onCooldown = false;
        }
        
        private void OnDestroy()
        {
            GameStateManager.Instance.OnPlayerDeath -= SetupWheel;
        }

        protected override void SectionSelected(DeathWheelSection section)
        {
            if (_loopCount > MaxLoops) return;
            
            base.SectionSelected(section);
            if (section.isRevive)
            {
                GameStateManager.Instance.OnPlayerRevive?.Invoke();
            }
            else
            {
                GameStateManager.Instance.OnGameOver?.Invoke();
            }
        }

        protected override void WheelLooped()
        {
            base.WheelLooped();
            if (_loopCount < MaxLoops) return;
            
            GameStateManager.Instance.OnGameOver?.Invoke();
        }
    }
}