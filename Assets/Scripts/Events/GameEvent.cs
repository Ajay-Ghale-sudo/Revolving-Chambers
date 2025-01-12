using System;
using UnityEngine;

namespace Events
{
    /// <summary>
    /// Interface for GameEvents.
    /// </summary>
    public interface IGameEvent
    {
        /// <summary>
        /// Invokes the GameEvent.
        /// </summary>
        /// <param name="invoker">GameObject that is invoking this event.</param>
        void Invoke(GameObject invoker = null);
    }

    /// <summary>
    /// Data for the GameEvent.
    /// </summary>
    [Serializable]
    public class GameEventData
    {
        
    }

    /// <summary>
    /// Base class for GameEvents.
    /// </summary>
    /// <typeparam name="T"><see cref="GameEventData"/> for the Event.</typeparam>
    public abstract class GameEvent<T> : ScriptableObject, IGameEvent where T : GameEventData, new()
    {
        /// <summary>
        /// Data for the GameEvent.
        /// </summary>
        [SerializeField]
        protected T data;
        
        public void Invoke(GameObject invoker = null)
        {
            OnInvoke(invoker);
        }
        
        /// <summary>
        /// Called when the <see cref="GameEvent"/> is invoked.
        /// </summary>
        /// <param name="invoker">GameObject that invoked this event.</param>
        protected virtual void OnInvoke(GameObject invoker = null)
        {
            Debug.Log("GameEvent Invoked: " + this.name);
        }
    }
    
    /// <summary>
    /// Default implementation of GameEvent that uses <see cref="GameEventData"/>.
    /// </summary>
    public abstract class GameEvent : GameEvent<GameEventData>
    {
    }
    
}