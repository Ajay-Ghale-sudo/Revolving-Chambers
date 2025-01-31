using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility;
using UI;

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
        /// Action for when a boss is first aggroed.
        /// </summary>
        public Action OnBossIntroStart;
        
        /// <summary>
        /// Action for when a boss fight starts.
        /// </summary>
        public Action OnBossFightStart;

        /// <summary>
        /// Action for when a boss dies.
        /// </summary>
        public Action OnBossDeath;

        /// <summary>
        /// Action for diamond boss' phase 1 ending
        /// </summary>
        public Action OnDiamondBossPhase1End;

        /// <summary>
        /// Action for diamond boss' phase 2 ending
        /// </summary>
        public Action OnDiamondBossPhase2End;
        
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

        /// <summary>
        /// Action for when the game is paused.
        /// </summary>
        public Action<bool> OnGamePause;

        /// <summary>
        /// Action for when the volume slider is changed
        /// </summary>
        public Action<float> OnVolumeChange;

        private void Start()
        {
            OnPlayerDeath += PlayerDeath;
            OnBossDeath += BossDeath;
            OnPlayerRevive += PlayerRevive;
            OnGameOver += GameOver;
            OnGameStart += GameStart;
            OnGamePause += PauseGame;
            OnVolumeChange += ChangeVolume;
        }

        private void OnDestroy()
        {
            OnPlayerDeath -= PlayerDeath;
            OnBossDeath -= BossDeath;
            OnPlayerRevive -= PlayerRevive;
            OnGameOver -= GameOver;
            OnGamePause -= PauseGame;
            OnVolumeChange -= ChangeVolume;
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

        /// <summary>
        /// Handle game start.
        /// </summary>
        private void GameStart()
        {
            Time.timeScale = 1f;
        }

        private void GameOver()
        {
            // TODO: Show game over screen
            DOTween.Clear();
            Time.timeScale = 1f;
            // Just reload scene for now, until we have UI to show game over screen.
            
            Invoke(nameof(LoadMainMenu), 3f);
        }
        
        private void LoadMainMenu()
        {
            // get main menu scene index
            SceneManager.LoadScene(0);
        }

        /// <summary>
        /// Pauses the timescale and DOTweens
        /// </summary>
        /// <param name="state"></param>
        private void PauseGame(bool state)
        {
            if (state) 
            { 
                Time.timeScale = 0.0f;
                DOTween.PauseAll();
            }
            else 
            { 
                Time.timeScale = 1f;
                DOTween.PlayAll();
            }
        }

        /// <summary>
        /// Change global volume
        /// </summary>
        /// <param name="vol"></param>
        private void ChangeVolume(float vol)
        {
            AudioListener.volume = vol;
        }
    }
}