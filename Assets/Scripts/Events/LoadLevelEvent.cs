using System;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Events
{
    /// <summary>
    /// Data for the LoadLevelEvent.
    /// </summary>
    [Serializable]
    public class LoadLevelEventData : GameEventData
    {
        /// <summary>
        /// Level to load.
        /// </summary>
        [SerializeField]
        public SceneReference level;
        
        /// <summary>
        /// LoadSceneMode for the level.
        /// </summary>
        [SerializeField]
        public LoadSceneMode loadSceneMode;
    }
    
    /// <summary>
    /// Event that loads a level.
    /// </summary>
    [CreateAssetMenu(fileName = "Data", menuName = "Game Events/Load Level", order = 0)]
    public class LoadLevelEvent : GameEvent<LoadLevelEventData>
    {
        protected override void OnInvoke(GameObject invoker = null)
        {
            base.OnInvoke(invoker);
            
            if (data.level == null || data.level.BuildIndex == -1)
            {
                Debug.LogWarning("No level to load in LoadLevelEvent");
                return;
            }
            
            SceneManager.LoadScene(data.level.BuildIndex, data.loadSceneMode);
        }
    }
}