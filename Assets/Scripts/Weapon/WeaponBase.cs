using UnityEngine;
using UnityEngine.Events;

namespace Weapon
{
    /// <summary>
    /// Interface for a weapon.
    /// </summary>
    public interface IWeapon
    {
        /// <summary>
        /// Fire the weapon.
        /// </summary>
        void Fire();
        
        /// <summary>
        /// Get the direction the weapon is firing.
        /// </summary>
        /// <returns>Normalized direction vector</returns>
        Vector3 GetFireDirection();
    }
    
    /// <summary>
    /// Base class for a weapon.
    /// </summary>
    public abstract class WeaponBase : MonoBehaviour, IWeapon
    {
        /// <summary>
        /// Event invoked when the weapon is fired.
        /// </summary>
        public UnityEvent OnFire;
        
        /// <summary>
        /// Fire the weapon.
        /// </summary>
        public virtual void Fire()
        {
            Debug.Log("Firing weapon");
        }
        
        public virtual Vector3 GetFireDirection()
        {
            // If there is no main camera, return the forward direction of the weapon
            if (Camera.main == null) return transform.forward;
            
            // Get cursor position in world space
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out var hit) ? (hit.point - transform.position).normalized : transform.forward;
        }
    }
}