﻿using System;
using UnityEngine;
using Utility;
using Weapon;

namespace UI
{
    /// <summary>
    /// Manager for UI.
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
       // UI Actions
       
       /// <summary>
       /// Action for when the player dies.
       /// </summary>
       public Action OnPlayerDeath;

       /// <summary>
       /// Action for when the player takes damage.
       /// </summary>
       public Action OnPlayerDamage;
       
       /// <summary>
       /// Action for when the player's health changes.
       /// </summary>
       public Action<int> OnPlayerHealthChange;
       
       /// <summary>
       /// Action for when a <see cref="Revolver"/>'s ammo changes.
       /// </summary>
       public Action<int, Revolver.RevolverChamber> OnRevolverAmmoChange;

       /// <summary>
       /// Action for when the chamber of the <see cref="Revolver"/> changes.
       /// </summary>
       public Action<int> OnChamberChanged;

       /// <summary>
       /// Action for when a Boss is spawned.
       /// 'string' Name of boss
       /// </summary>
       public Action<string> OnBossSpawned;

       /// <summary>
       /// Action for when Boss health changes.
       /// 'float' Boss health represented in 0.0f - 1.0f
       /// </summary>
       public Action<float> OnBossHealthChange;
    }
}