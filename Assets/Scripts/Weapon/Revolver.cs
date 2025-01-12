using System;
using System.Collections.Generic;
using NUnit.Framework;
using Props;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Weapon
{
    /// <summary>
    /// Revolver weapon.
    /// </summary>
    public class Revolver : WeaponBase
    {
        /// <summary>
        /// Chamber of the revolver.
        /// </summary>
        [Serializable]
        public class RevolverChamber
        {
            /// <summary>
            /// Event invoked when the chamber is loaded.
            /// </summary>
            public UnityEvent OnLoaded;
            
            /// <summary>
            /// Event invoked when the chamber is fired.
            /// </summary>
            public UnityEvent OnFire;
            
            /// <summary>
            /// Whether the chamber is empty.
            /// </summary>
            public bool IsEmpty => Ammo == null;
            
            /// <summary>
            /// The ammo in the chamber.
            /// </summary>
            public Ammo Ammo { get; private set; }
            
            /// <summary>
            /// Load the chamber with the desired <see cref="Ammo"/>
            /// </summary>
            /// <param name="ammo">The <see cref="Ammo"/> to load into the chamber.</param>
            internal void LoadChamber(Ammo ammo)
            {
                // TODO: Should we check if the chamber is already loaded?
                Ammo = ammo;
                OnLoaded?.Invoke();
            }

            /// <summary>
            ///  Fire the <see cref="Ammo"/> in the chamber.
            /// </summary>
            /// <returns><see cref="GameObject"/> of the created bullet</returns>
            protected internal GameObject Fire(out Ammo ammo)
            {
                ammo = Ammo;
                if (IsEmpty) return null;
                OnFire?.Invoke();
                var bullet = BulletManager.Instance.SpawnBullet(ammo, Vector3.zero, Quaternion.identity);
                Ammo = null;
                return bullet.gameObject;
            }
        }
        
        /// <summary>
        /// Event invoked when the chamber is changed.
        /// </summary>
        public UnityEvent OnChamberChanged;
        
        /// <summary>
        /// List of chambers in the revolver.
        /// </summary>
        private List<RevolverChamber> chambers;
        
        /// <summary>
        /// Transform of the muzzle. Where the bullet will be fired from.
        /// </summary>
        [SerializeField]
        private Transform muzzleTransform;
        
        /// <summary>
        /// The current chamber index.
        /// </summary>
        public int CurrentChamberIndex { get; private set; }
        
        /// <summary>
        /// The number of chambers the revolver has.
        /// </summary>
        [SerializeField]
        private int chamberCount = 6;
        
        /// <summary>
        /// The fire rate of the revolver.
        /// </summary>
        [SerializeField]
        private int fireRate = 1;

        /// <summary>
        /// Wheel used for reloading.
        /// </summary>
        [SerializeField] private ReloadWheel reloadWheel;
        
        public override void Fire()
        {
            var bullet = CurrentChamber.Fire(out var ammo);
            if (bullet != null && ammo != null)
            {
                bullet.transform.position = muzzleTransform.position;
                bullet.transform.rotation = Quaternion.LookRotation(GetFireDirection());
                var rb = bullet.GetComponent<Rigidbody>();
                if (rb == null) return;
                rb.linearVelocity = muzzleTransform.forward * ammo.velocity;
            }
            NextChamber();
        }
        
        /// <summary>
        /// Trigger the weapon to reload.
        /// </summary>
        public override void Reload()
        {
            OnReload?.Invoke();
        }

        private void Awake()
        {
            InitChambers();
            
            ReloadManager.Instance.OnLoadAmmo += LoadCurrentChamber;
        }

        /// <summary>
        /// Initialize the chambers of the revolver.
        /// </summary>
        private void InitChambers()
        {
            chambers = new List<RevolverChamber>();
            for (var i = 0; i < chamberCount; i++)
            {
                var chamber = new RevolverChamber();
                chambers.Add(chamber);
            }
        }
        
        /// <summary>
        /// The current chamber of the revolver.
        /// </summary>
        public RevolverChamber CurrentChamber => chambers[CurrentChamberIndex];
        
        /// <summary>
        /// Get the chamber at the desired index.
        /// </summary>
        /// <param name="index">Index of the Chamber to get.</param>
        /// <returns>The <see cref="RevolverChamber"/> at the desired index.</returns>
        public RevolverChamber GetChamber(int index)
        {
            return chambers[index];
        }
        
        /// <summary>
        /// Load the chamber with the desired <see cref="Ammo"/>
        /// </summary>
        /// <param name="ammo">Ammo to load into the Chamber</param>
        /// <param name="index">Which Chamber to load into, defaults to current chamber if not set.</param>
        public void LoadChamber(Ammo ammo, int? index = null)
        {
            var targetChamber = index ?? CurrentChamberIndex;
            if (targetChamber < 0 || targetChamber >= chambers.Count) return;
            chambers[targetChamber].LoadChamber(ammo);
        }
        
        /// <summary>
        /// Load the specified ammo into the current chamber.
        /// </summary>
        /// <param name="ammo">Ammo to load</param>
        private void LoadCurrentChamber(Ammo ammo)
        {
            LoadChamber(ammo, CurrentChamberIndex);
        }
        
        /// <summary>
        /// Move to the next chamber.
        /// </summary>
        public void NextChamber()
        {
            CurrentChamberIndex = (CurrentChamberIndex + 1) % chambers.Count;
            OnChamberChanged?.Invoke();
        }
    }
}