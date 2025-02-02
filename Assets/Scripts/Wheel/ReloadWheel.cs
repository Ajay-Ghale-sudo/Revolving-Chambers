using System;
using System.Collections.Generic;
using Events;
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
        
        /// <summary>
        /// Cooldown before next reload trigger.
        /// HACK: Used because of double trigger bug.
        /// </summary>
        private bool _cooldown = false;
        
        /// <summary>
        /// Ammo loadouts for the wheel.
        /// TODO: configured in editor for now, but should be driven from the game itself.
        /// </summary>
        [SerializeField] private List<AmmoLoadout> ammoLoadouts;

        protected override void SetupWheel()
        {
            // Update the wheel sections with the ammo loadouts.
            ChooseRandomAmmoLoadout();
            
            GameStateManager.Instance.OnPlayerRevive -= SetupWheel;
            GameStateManager.Instance.OnPlayerDeath += PlayerDied;
            
            ReloadManager.Instance.OnSpinStart += SpinWheel;
            ReloadManager.Instance.OnSpinEnd += StopWheel;
        }
        
        /// <summary>
        /// Choose a random ammo loadout.
        /// </summary>
        private void ChooseRandomAmmoLoadout()
        {
            wheelSections.Clear();
            var loadout = ammoLoadouts[UnityEngine.Random.Range(0, ammoLoadouts.Count)];
            wheelSections.AddRange(loadout.ammoSections);
            
            CreateWheelSections();
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
           GameStateManager.Instance.OnPlayerRevive -= PlayerRevived;
           ReloadManager.Instance.OnSpinStart -= SpinWheel;
           ReloadManager.Instance.OnSpinEnd -= StopWheel;
        }

        protected override void SectionSelected(AmmoWheelSection section)
        {
            if (_cooldown) return;
            
            _cooldown = true;
            Invoke(nameof(DisableCooldown), 1f);
            base.SectionSelected(section);
            ReloadManager.Instance.OnLoadAmmo?.Invoke(_selectedSection.Reward);
            _selectedSection.Reward.loadSound?.Invoke();
            ChooseRandomAmmoLoadout();
        }
        
        /// <summary>
        /// Disable the cooldown.
        /// </summary>
        protected void DisableCooldown()
        {
            _cooldown = false;
        }
    }
}