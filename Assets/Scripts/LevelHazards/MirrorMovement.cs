using System;
using DG.Tweening;
using State;
using UnityEngine;

namespace LevelHazards
{
    public class MirrorMovement : MonoBehaviour
    {
        private Array mirrors;

        private Tweener rotationTweener;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            rotationTweener = transform.DORotate(new Vector3(0f, -360.0f, 0.0f), 30f, RotateMode.FastBeyond360).SetLoops(-1).SetRelative(true).SetEase(Ease.Linear);
            rotationTweener.timeScale = 0.0f;
            
            GameStateManager.Instance.OnBossIntroStart += HandleBossIntroStart;
            GameStateManager.Instance.OnBossFightStart += HandleBossFightStart;
            GameStateManager.Instance.OnBossDeath += HandleBossDeath;
        }

        /// <summary>
        /// Logic for when a boss's intro starts.
        /// </summary>
        private void HandleBossIntroStart()
        {
            DOTween.To(
                () => rotationTweener.timeScale,
                newTimeScale => rotationTweener.timeScale = newTimeScale,
                5.0f,
                5.0f
            );
        }

        /// <summary>
        /// Logic for when a boss's intro ends and the fight starts.
        /// </summary>
        private void HandleBossFightStart()
        {
            DOTween.To(
                () => rotationTweener.timeScale,
                newTimeScale => rotationTweener.timeScale = newTimeScale,
                1.0f,
                5.0f
            );
        }

        /// <summary>
        /// Cleanup when the boss dies.
        /// </summary>
        private void HandleBossDeath()
        {
            DOTween.To(
                () => rotationTweener.timeScale,
                newTimeScale => rotationTweener.timeScale = newTimeScale,
                0.0f,
                10.0f
            );
        }

        private void OnDestroy()
        {
            GameStateManager.Instance.OnBossIntroStart -= HandleBossIntroStart;
            GameStateManager.Instance.OnBossFightStart -= HandleBossFightStart;
            GameStateManager.Instance.OnBossDeath -= HandleBossDeath;
        }
    }
}