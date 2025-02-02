using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// UI for boss phase. Manages displaying boss phase.
    /// </summary>
    public class UI_BossPhase : MonoBehaviour
    {
        /// <summary>
        /// List of phases.
        /// </summary>
        List<RawImage> _phases = new();
        
        /// <summary>
        /// Prefab for the phase.
        /// </summary>
        [SerializeField] private GameObject phasePrefab;

        /// <summary>
        /// Active color of the phase.
        /// </summary>
        [SerializeField] private Color activeColor = Color.white;
        
        /// <summary>
        /// Inactive color of the phase.
        /// </summary>
        [SerializeField] private Color inactiveColor = Color.gray;

        private void Awake()
        {
            BindUIEvents();
        }

        private void OnDestroy()
        {
            UnbindUIEvents();
        }

        /// <summary>
        /// Bind UI events.
        /// </summary>
        private void BindUIEvents()
        {
            UIManager.Instance.OnBossPhaseChange += UpdateActivePhase;
            UIManager.Instance.OnBossMaxPhasesChange += UpdateMaxPhases;
        }

        /// <summary>
        /// Unbind UI events.
        /// </summary>
        private void UnbindUIEvents()
        {
            UIManager.Instance.OnBossPhaseChange -= UpdateActivePhase;
            UIManager.Instance.OnBossMaxPhasesChange -= UpdateMaxPhases;
        }

        /// <summary>
        /// Update the active phase.
        /// </summary>
        /// <param name="phase">The active phase</param>
        private void UpdateActivePhase(int phase)
        {
            for (var index = 0; index < _phases.Count; index++)
            {
                _phases[index].color = index < phase ? activeColor : inactiveColor;
            }
        }

        /// <summary>
        /// Update the max phases.
        /// </summary>
        /// <param name="maxPhases"></param>
        private void UpdateMaxPhases(int maxPhases)
        {
            foreach (var phase in _phases)
            {
                Destroy(phase.gameObject);
            }

            _phases.Clear();

            for (var index = 0; index < maxPhases; index++)
            {
                var phase = Instantiate(phasePrefab, transform);
                if (!phase.TryGetComponent<RawImage>(out var image)) return;
                image.color = activeColor;
                _phases.Add(image);
            }
        }
    }
}