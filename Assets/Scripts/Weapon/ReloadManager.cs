﻿using System;
using UnityEngine;
using UnityEngine.Events;
using Utility;

namespace Weapon
{
    /// <summary>
    /// Manager for reloading weapons.
    /// </summary>
    public class ReloadManager : Singleton<ReloadManager>
    {
        /// <summary>
        /// Action to start spinning the reload wheel.
        /// </summary>
        public UnityAction OnSpinStart;
        
        /// <summary>
        /// Action to stop spinning the reload wheel.
        /// </summary>
        public UnityAction OnSpinEnd;
        
        /// <summary>
        /// Action invoked when the reload wheel updates.
        /// </summary>
        public UnityAction<int> OnSpinUpdate;
        
        /// <summary>
        /// Action invoked when a section is added to the reload wheel.
        /// </summary>
        public UnityAction<int, int, Color> OnSpinSectionAdded;
        
        /// <summary>
        /// Action invoked when all sections are cleared from the reload wheel.
        /// </summary>
        public UnityAction OnClearSections;

        /// <summary>
        /// Action invoked when ammo is loaded.
        /// </summary>
        public Action<Ammo> OnLoadAmmo;

        /// <summary>
        /// Register a weapon with the reload manager.
        /// </summary>
        /// <param name="weapon"></param>
        public void RegisterWeapon(WeaponBase weapon)
        {
            weapon.OnReload += OnSpinStart;
        }
        
        /// <summary>
        /// Deregister a weapon with the reload manager.
        /// </summary>
        /// <param name="weapon"></param>
        public void DeregisterWeapon(WeaponBase weapon)
        {
            weapon.OnReload -= OnSpinStart;
        }
    }
}