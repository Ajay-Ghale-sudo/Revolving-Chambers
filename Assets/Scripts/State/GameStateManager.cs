using System;
using UnityEngine;
using Utility;

namespace State
{
    /// <summary>
    /// Manager for game state.
    /// </summary>
    public class GameStateManager : Singleton<GameStateManager> 
    {
        /// <summary>
        /// Action for when the game starts.
        /// </summary>
        public Action OnGameStart;
        
        /// <summary>
        /// Action for when the game ends.
        /// </summary>
        public Action OnGameEnd;

        /// <summary>
        /// Action for when a boss dies.
        /// </summary>
        public Action OnBossDeath;
        
        /// <summary>
        /// Action for when the player dies.
        /// </summary>
        public Action OnPlayerDeath;

        private void Start()
        {
            OnPlayerDeath += PlayerDeath;
            OnBossDeath += BossDeath;
        }

        /// <summary>
        /// Handle player death.
        /// </summary>
        private void PlayerDeath()
        {
            // Just reload scene for now, until we have UI to show game over screen.
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
        }
        
        /// <summary>
        /// Handle boss death.
        /// </summary>
        private void BossDeath()
        {
            // Just reload scene for now, until we have UI to show game over screen.
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
        }
    }
}