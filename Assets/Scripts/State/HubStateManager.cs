using System;
using System.Collections.Generic;
using UnityEngine;

namespace State
{
    /// <summary>
    ///  Manages the state of the hub.
    /// </summary>
    public class HubStateManager : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> portals;
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
            if (GameStateManager.Instance.lastKilledBoss == BossType.Craps ||
                GameStateManager.Instance.lastKilledBoss == BossType.Diamond)
            {
                UnlockPortal();
            }
            playerTransform.position = GameStateManager.Instance.lastKilledBoss switch
            {
                BossType.Craps => crapsBossTransform.position,
                BossType.Diamond => diamondTransform.position,
                BossType.Roulette => rouletteBossTransform.position,
                _ => playerTransform.position
            };
        }

        private void UnlockPortal()
        {
            if (GameStateManager.Instance.lastKilledBoss == BossType.Craps)
            {
                GameObject diamondPortal = portals[0];
                diamondPortal.SetActive(false);
                print("Killed craps boss, unlocking portal.");
            }

            if (GameStateManager.Instance.lastKilledBoss == BossType.Diamond)
            {
                GameObject roulettePortal = portals[1];
                roulettePortal.SetActive(false);
                print("Killed diamond boss, unlocking portal.");
            }
        }
    }
}