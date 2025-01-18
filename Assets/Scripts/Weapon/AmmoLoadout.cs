using System.Collections.Generic;
using UnityEngine;
using Wheel;

namespace Weapon
{
    /// <summary>
    /// Sections for the Reload Wheel.
    /// </summary>
    [CreateAssetMenu(fileName = "data", menuName = "Scriptable Objects/Ammo Loadout", order = 0)]
    public class AmmoLoadout : ScriptableObject
    {
        /// <summary>
        /// Ammo sections for the Reload Wheel.
        /// </summary>
        [SerializeField]
        public List<AmmoWheelSection> ammoSections;
    }
}