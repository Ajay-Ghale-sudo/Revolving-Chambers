using System;
using Audio;
using Interfaces;
using UI;
using UnityEngine;

namespace Boss.Craps
{
    /// <summary>
    /// Base class for all bosses.
    /// </summary>
    public abstract class BossBase : Damageable
    {
        /// <summary>
        /// Max health of the boss.
        /// </summary>
        [SerializeField] protected int phaseCount = 1;

        /// <summary>
        /// Current phase of the boss.
        /// </summary>
        protected int currentPhase = 1;
        
        /// <summary>
        /// Initialize the boss.
        /// </summary>
        protected virtual void InitBoss()
        {
            currentPhase = phaseCount;
            
            UIManager.Instance.OnBossSpawned?.Invoke(name);
            UIManager.Instance.OnBossHealthChange?.Invoke(health / maxHealth);
            UIManager.Instance.OnBossMaxPhasesChange?.Invoke(phaseCount);
            UIManager.Instance.OnBossPhaseChange?.Invoke(currentPhase);
            
        }
        
        /// <summary>
        /// Move to the next boss phase.
        /// </summary>
        public void NextPhase()
        {
            --currentPhase;
            UIManager.Instance.OnBossPhaseChange?.Invoke(currentPhase);
        }
    }
}