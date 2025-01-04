using System;
using UnityEngine;

namespace Events
{
    /// <summary>
    /// Data for the <see cref="LocationAudioEvent"/>.
    /// </summary>
    [Serializable]
    public class LocationAudioEventData : AudioEventData
    {
        /// <summary>
        /// Whether to use the invokers position when playing the audio.
        /// </summary>
        [SerializeField]
        [Tooltip("Play audio at the invokers position")]
        public bool useInvokersPosition = true;
        
        /// <summary>
        /// Location of the audio source.
        /// </summary>
        [SerializeField]
        [Tooltip("Location to play the audio")]
        public Vector3 location = Vector3.zero;
    }
    
    /// <summary>
    /// Event that plays audio at a specific location.
    /// </summary>
    [CreateAssetMenu(fileName = "data", menuName = "Game Events/Location Audio", order = 1)]
    public class LocationAudioEvent : GameEvent<LocationAudioEventData>
    {
        /// <summary>
        /// Event that is triggered when the <see cref="LocationAudioEvent"/> is invoked.
        /// </summary>
        public static Action<LocationAudioEventData> OnLocationAudioEvent;
        
        protected override void OnInvoke(GameObject invoker = null)
        {
            base.OnInvoke(invoker);
            
            data.location = data.useInvokersPosition && invoker ? invoker.transform.position : data.location;
            OnLocationAudioEvent?.Invoke(data);
        }
    }
}