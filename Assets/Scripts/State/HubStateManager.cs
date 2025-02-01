using System;
using UnityEngine;

namespace State
{
    /// <summary>
    ///  Manages the state of the hub.
    /// </summary>
    public class HubStateManager : MonoBehaviour
    {
        
        /// <summary>
        /// Transform of the player.
        /// </summary>
        [SerializeField]
        private Transform playerTransform;
        
        /// <summary>
        /// Transform of the craps boss portal.
        /// </summary>
        [SerializeField]
        private Transform crapsBossTransform;
        
        /// <summary>
        /// Transform of the diamond boss portal.
        /// </summary>
        [SerializeField]
        private Transform diamondTransform;
        
        /// <summary>
        /// Transform of the roulette boss portal.
        /// </summary>
        [SerializeField]
        private Transform rouletteBossTransform;


        private void Awake()
        {
            LoadPlayerToPosition();
        }

        /// <summary>
        /// Load the player to the last killed boss position.
        /// </summary>
        private void LoadPlayerToPosition()
        {
            playerTransform.position = GameStateManager.Instance.lastKilledBoss switch
            {
                BossType.Craps => crapsBossTransform.position,
                BossType.Diamond => diamondTransform.position,
                BossType.Roulette => rouletteBossTransform.position,
                _ => playerTransform.position
            };
        }
    }
}