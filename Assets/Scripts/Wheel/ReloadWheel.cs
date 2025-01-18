using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using State;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Weapon;

namespace Wheel
{
    /// <summary>
    /// Section of the wheel. Reward is Ammo to load into the weapon.
    /// </summary>
    [Serializable]
    public class AmmoWheelSection : WheelSection
    {
        public Ammo Reward;
        public override Color SectionColor => Reward?.color ?? Color.magenta;
    }
    
    /// <summary>
    /// Reload wheel for selecting ammo.
    /// </summary>
    public class ReloadWheel : RewardWheel<AmmoWheelSection>
    {
        protected override void SetupWheel()
        {
            base.SetupWheel();
            
            GameStateManager.Instance.OnPlayerRevive -= SetupWheel;
            GameStateManager.Instance.OnPlayerDeath += PlayerDied;
            
            ReloadManager.Instance.OnSpinStart += SpinWheel;
            ReloadManager.Instance.OnSpinEnd += StopWheel;
        }

        /// <summary>
        /// Unbinds events on PlayerDeath.
        /// </summary>
        private void PlayerDied()
        {
           ReloadManager.Instance.OnSpinStart -= SpinWheel;
           ReloadManager.Instance.OnSpinEnd -= StopWheel;
           GameStateManager.Instance.OnPlayerDeath -= PlayerDied;
           
           GameStateManager.Instance.OnPlayerRevive += PlayerRevived;
        }

        /// <summary>
        /// Setup the wheel on player revive.
        /// </summary>
        private void PlayerRevived()
        {
            GameStateManager.Instance.OnPlayerRevive -= PlayerRevived;
            SetupWheel();
        }

        private void OnDestroy()
        {
           ReloadManager.Instance.OnSpinStart -= SpinWheel;
           ReloadManager.Instance.OnSpinEnd -= StopWheel;
        }

        protected override void SectionSelected(AmmoWheelSection section)
        {
            base.SectionSelected(section);
            ReloadManager.Instance.OnLoadAmmo?.Invoke(_selectedSection.Reward);
        }
    }
}