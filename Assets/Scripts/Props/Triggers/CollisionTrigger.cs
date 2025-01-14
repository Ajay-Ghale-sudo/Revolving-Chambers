using System;
using System.Collections.Generic;
using Events;
using UnityEngine;

namespace Props.Triggers
{
    /// <summary>
    /// Trigger that activates when a collider enters the trigger.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CollisionTrigger : TriggerBase 
    {
        /// <summary>
        /// Should the events be invoked with the collider that entered the trigger?
        /// </summary>
        [Tooltip("Should the events be invoked with the collider that entered the trigger?")]
        [SerializeField] private bool invokeWithCollider = false;
        
        /// <summary>
        /// The currently colliding gameobject.
        /// </summary>
        private GameObject _currentlyColliding;
        
        /// <summary>
        /// When a collider enters the trigger, invoke the events.
        /// </summary>
        /// <param name="other">The Collider colliding with this trigger.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!string.IsNullOrEmpty(tagFilter) && !other.CompareTag(tagFilter)) return;
            _currentlyColliding = other.gameObject;
            InvokeEvents();
        }
        
        /// <summary>
        /// When a collider exits the trigger, set the currently colliding gameobject to null.
        /// </summary>
        /// <param name="other">The Collider leaving this trigger.</param>
        private void OnTriggerExit(Collider other)
        {
            if (!string.IsNullOrEmpty(tagFilter) && !other.CompareTag(tagFilter)) return;
            if (_currentlyColliding == other.gameObject)
                _currentlyColliding = null;
        }

        protected override void InvokeEvents()
        {
            if (!invokeWithCollider) base.InvokeEvents();
            else
            {
                foreach (var gameEvent in events)
                {
                    gameEvent.Invoke(_currentlyColliding);
                }
            }
        }
    }
}