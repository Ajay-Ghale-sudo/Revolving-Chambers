using System;
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
        /// Reload the weapon.
        /// </summary>
        void Reload();
        
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
        public UnityAction OnFire;
        
        /// <summary>
        ///  Event invoked when the weapon is reloaded.
        /// </summary>
        public UnityAction OnReload;

        private void Start()
        {
            ReloadManager.Instance.RegisterWeapon(this); 
        }

        private void OnDestroy()
        {
            ReloadManager.Instance.DeregisterWeapon(this);
        }

        /// <summary>
        /// Fire the weapon.
        /// </summary>
        public virtual void Fire()
        {
            Debug.Log("Firing weapon");
        }

        public virtual void Reload()
        {
            
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