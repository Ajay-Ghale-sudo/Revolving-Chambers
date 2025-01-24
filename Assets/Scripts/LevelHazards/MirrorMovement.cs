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
            GameStateManager.Instance.OnDiamondBossPhase1End += HandleDiamondBossPhase1End;
            GameStateManager.Instance.OnDiamondBossPhase2End += HandleDiamondBossPhase2End;
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
        /// Handler for switching from diamond boss phase 1 to phase 2.
        /// </summary>
        private void HandleDiamondBossPhase1End()
        {
        }

        /// <summary>
        /// Handler for switching from diamond boss phase 2 to phase 3.
        /// </summary>
        private void HandleDiamondBossPhase2End()
        {
            var currentScale = transform.localScale;
            var targetScale = new Vector3(currentScale.x * 0.75f, currentScale.y, currentScale.z * 0.75f);
            
            DOTween.To(
                () => transform.localScale,
                xyz => transform.localScale = new Vector3(xyz.x, transform.localScale.y, xyz.z),
                targetScale,
                20.0f
            ).SetEase(Ease.OutSine);

            DOTween.To(
                () => rotationTweener.timeScale,
                newTimeScale => rotationTweener.timeScale = newTimeScale,
                5.0f,
                60.0f
            ).SetEase(Ease.InSine);
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

            var currentScale = transform.localScale;
            var targetScale = new Vector3(currentScale.x * 4.0f/3.0f, currentScale.y, currentScale.z * 4.0f/3.0f);
            
            DOTween.To(
                () => transform.localScale,
                xyz => transform.localScale = new Vector3(xyz.x, transform.localScale.y, xyz.z),
                targetScale,
                20.0f
            ).SetEase(Ease.InOutSine);
        }

        private void OnDestroy()
        {
            GameStateManager.Instance.OnBossIntroStart -= HandleBossIntroStart;
            GameStateManager.Instance.OnBossFightStart -= HandleBossFightStart;
            GameStateManager.Instance.OnBossDeath -= HandleBossDeath;
        }
    }
}