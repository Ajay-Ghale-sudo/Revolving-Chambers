using Interfaces;
using State;
using UnityEngine;
using Weapon;

namespace Events
{
    /// <summary>
    /// Event for a life steal bullet. Heals when hitting the same object multiple times in a row.
    /// </summary>
    [CreateAssetMenu(fileName = "Data", menuName = "Game Events/Life Steal Bullet", order = 0)]
    public class LifeStealBulletEvent : BulletCollisionEvent
    {
        /// <summary>
        /// The ID of the last object hit.
        /// </summary>
        private static int lastHitObjectID;
        
        /// <summary>
        /// The number of consecutive hits on the same object.
        /// </summary>
        private static int consecutiveHits = 0;
        
        /// <summary>
        /// The time of the last hit.
        /// </summary>
        private static float lastHitTime;
        
        /// <summary>
        /// The amount of health to steal.
        /// </summary>
        [SerializeField] private int lifeStealAmount = 2;
        
        /// <summary>
        /// How long the player has to hit the same object to trigger life steal.
        /// </summary>
        [SerializeField] private float lifeStealTimeWindow = 2.5f;
        
        /// <summary>
        /// How many hits the player needs to trigger life steal.
        /// </summary>
        [SerializeField] private int lifeStealThreshold = 3;
        
        protected override void OnCollision(Bullet bullet)
        {
            base.OnCollision(bullet);

            if (!bullet.ObjectHit || !bullet.ObjectHit.TryGetComponent<IDamageable>(out var damageableComponent))
            {
                ResetHits();
                return;
            }
            var timeSinceLastHit = Time.time - lastHitTime;
            var objectID = bullet.ObjectHit.GetInstanceID();
            if (objectID == lastHitObjectID && timeSinceLastHit <= lifeStealTimeWindow)
            {
                if (++consecutiveHits >= lifeStealThreshold)
                {
                    GameStateManager.Instance.OnPlayerHeal?.Invoke(lifeStealAmount);
                    consecutiveHits = 0;
                }
            }
            else
            {
                consecutiveHits = 1;
                lastHitObjectID = objectID;
            }
            
            lastHitTime = Time.time;
            
        }
        
        /// <summary>
        /// Reset the hit counter.
        /// </summary>
        private void ResetHits()
        {
            consecutiveHits = 0;
            lastHitObjectID = 0;
            lastHitTime = 0;
        }
    }
}