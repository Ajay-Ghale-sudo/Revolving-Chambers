using System;
using UnityEngine;
using Utility;

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
    }
}