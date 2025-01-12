using UnityEngine;
using System.Linq;

namespace UI
{
    /// <summary>
    /// Handles effects for the bullet gameobject used for UI.
    /// Visibility is handled by turning on/off the renderer. Does not call gameobject.SetActive()
    /// </summary>
    public class UI_Bullet : MonoBehaviour
    {
        /// <summary>
        /// Cache renderer for easy access. Cached on Start()
        /// </summary>
        private Renderer _renderer;

        /// <summary>
        /// Shared material for the colour changing part of the bullet.
        /// Reference to the original material. Cached on Start()
        /// </summary>
        private Material[] _originalMats;

        /// <summary>
        /// Instanced material that should be destroyed manually.
        /// Used for changing this object's material through script.
        /// </summary>
        private Material[] _instancedMats;

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
            if(_renderer != null)
            {
                //Cache original materials
                _originalMats = _renderer.sharedMaterials;
            }
        }

        private void OnDestroy()
        {
            //Instanced materials are not managed by Unity
            //Make sure to destroy the material
            DestroyInstancedMaterials();
        }

        /// <summary>
        /// Changes the colour of the bullet.
        /// </summary>
        /// <param name="color">Colour to change to</param>
        /// <param name="emission">Should the material be emissive</param>
        public void SetColour(Color color, bool emission)
        {
            //Requires a renderer to work
            if (_renderer == null) return;

            ResetMaterials();

            //Clone all materials
            _instancedMats = _renderer.materials;

            foreach (Material mat in _instancedMats)
            {
                //Find "BulletType" material
                if (mat.name == "BulletType (Instance)")
                {
                    //Set emission and colour on instanced "BulletType" material
                    mat.SetColor("_BaseColor", color);

                    if (emission)
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", color);
                        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
                    }
                }
            }
        }

        /// <summary>
        /// Reset materials to their original shared materials
        /// </summary>
        public void ResetMaterials()
        {
            //Requires renderer
            if (_renderer == null) return;

            _renderer.materials = _originalMats;

            DestroyInstancedMaterials();
        }

        /// <summary>
        /// Sets the visibility of the mesh.
        /// </summary>
        /// <param name="state">Mesh on/off</param>
        public void SetVisible(bool state)
        {
            if (_renderer == null) return;

            _renderer.enabled = state;
        }

        /// <summary>
        /// Destroy all instanced materials.
        /// Unity does not manage these. Must be destroyed manually.
        /// </summary>
        void DestroyInstancedMaterials()
        {
            if (_instancedMats != null)
            {
                foreach (Material mat in _instancedMats)
                {
                    Destroy(mat);
                }
            }
        }
    }
}
