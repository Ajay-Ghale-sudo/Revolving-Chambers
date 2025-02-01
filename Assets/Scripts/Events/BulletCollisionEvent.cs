using UnityEngine;
using Weapon;

namespace Events
{
    /// <summary>
    /// Data for a bullet collision event.
    /// </summary>
    public class BulletCollisionEventData : GameEventData
    {
        
    }
    
    /// <summary>
    /// Event for when a bullet collides with something.
    /// </summary>
    [CreateAssetMenu(fileName = "Data", menuName = "Game Events/Bullet Collision", order = 0)]
    public class BulletCollisionEvent : GameEvent<BulletCollisionEventData>
    {
        protected override void OnInvoke(GameObject invoker = null)
        {
            base.OnInvoke(invoker);
            
            if (invoker?.GetComponent<Bullet>() is { } bullet)
            {
                OnCollision(bullet);
            }
        }
        
        /// <summary>
        /// Called when a bullet collides with something.
        /// </summary>
        /// <param name="bullet">The bullet that collided with something.</param>
        protected virtual void OnCollision(Bullet bullet)
        {
            if (!bullet) return;
        }
    }
}