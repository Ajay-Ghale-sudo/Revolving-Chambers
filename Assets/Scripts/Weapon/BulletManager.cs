using UnityEngine;
using Utility;

namespace Weapon
{
    /// <summary>
    /// Manager for bullets.
    /// </summary>
    public class BulletManager : Singleton<BulletManager> 
    {
        /// <summary>
        /// Spawn a bullet given the ammo, position, and rotation.
        /// </summary>
        /// <param name="ammo">Ammo of the bullet to spawn.</param>
        /// <param name="position">Where the bullet should be spawned</param>
        /// <param name="rotation">Rotation to apply to the spawned bullet</param>
        /// <returns>The spawned bullet</returns>
        public Bullet SpawnBullet(Ammo ammo, Vector3 position, Quaternion rotation)
        {
            var bulletObject = Instantiate(ammo.projectilePrefab, position, rotation);
            
            if (!bulletObject.TryGetComponent(out Bullet bullet)) return null;
            bullet.SetAmmo(ammo);
            
            if (bulletObject.TryGetComponent(out Rigidbody rb))
            {
                rb.linearVelocity = bulletObject.transform.forward * ammo.velocity;
            }
            
            return bullet;
        }
    }
}