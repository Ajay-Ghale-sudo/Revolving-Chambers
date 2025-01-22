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
        
        /// <summary>
        /// Action for when the player revives.
        /// </summary>
        public Action OnPlayerRevive;

        /// <summary>
        /// Action for when the game is over.
        /// </summary>
        public Action OnGameOver;

        private void Start()
        {
            OnPlayerDeath += PlayerDeath;
            OnBossDeath += BossDeath;
            OnPlayerRevive += PlayerRevive;
            OnGameOver += GameOver;
        }

        /// <summary>
        /// Handle player death.
        /// </summary>
        private void PlayerDeath()
        {
            // Just reload scene for now, until we have UI to show game over screen.
            // var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            // UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
            
            // Slow down time and show death wheel
            Time.timeScale = 0.01f;
        }
        
        /// <summary>
        /// Handle boss death.
        /// </summary>
        private void BossDeath()
        {
        }

        /// <summary>
        /// Handle player revival.
        /// </summary>
        private void PlayerRevive()
        {
            Time.timeScale = 1f;
        }

        private void GameOver()
        {
            // TODO: Show game over screen
            
            Time.timeScale = 1f;
            // Just reload scene for now, until we have UI to show game over screen.
            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
        }
    }
}