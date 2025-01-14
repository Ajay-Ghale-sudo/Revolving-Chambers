using System.Collections.Generic;
using Events;
using UnityEngine;
using UnityEngine.Events;

namespace Props.Triggers
{
    /// <summary>
    /// Base class for triggers.
    /// </summary>
    public abstract class TriggerBase : MonoBehaviour
    {
        /// <summary>
        /// Tag filter for the trigger.
        /// </summary>
        [Tooltip("Tag filter for the trigger.")]
        [SerializeField] protected string tagFilter;

        /// <summary>
        /// Should the trigger only be activated once?
        /// </summary>
        [Tooltip("Should the trigger only be activated once? Will destroy once triggered.")]
        [SerializeField] protected bool triggerOnce = false;
        
        /// <summary>
        /// Events to invoke when the trigger is activated.
        /// </summary>
        [Tooltip("Events to invoke when the trigger is activated.")]
        [SerializeField] protected List<GameEvent> events;
        
        /// <summary>
        /// UnityEvent to invoke when the trigger is activated.
        /// </summary>
        [Tooltip("Events to invoke when the trigger is activated.")]
        public UnityEvent OnTriggered;

        /// <summary>
        /// Activates the trigger.
        /// </summary>
        public void Trigger()
        {
            InvokeEvents();
            OnTriggered?.Invoke();
            if (triggerOnce)
            {
                // Give time for events to process before destroying the trigger.
                Invoke(nameof(Destroy), 0.1f);
            }
        }
        
        /// <summary>
        /// Invokes the <see cref="events"/>.
        /// </summary>
        protected virtual void InvokeEvents()
        {
            foreach (var gameEvent in events)
            {
                gameEvent.Invoke(gameObject);
            }

        }
    }
}