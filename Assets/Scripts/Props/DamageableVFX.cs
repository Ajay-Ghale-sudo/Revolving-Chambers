using UnityEngine;
using System.Collections;

namespace Props
{
    /// <summary>
    /// MonoBehaviour that handles visual effects for IDamageable objects.
    /// (Flash Colour, Flicker)
    /// TODO: Display damage text effect
    /// </summary>
    public class DamageableVFX : MonoBehaviour
    {
        /// <summary>
        /// Original material on this gameobject at Start().
        /// </summary>
        Material _originalSharedMaterial;

        /// <summary>
        /// Instanced material that should be destroyed manually.
        /// Used for changing this object's material through script.
        /// </summary>
        Material _instancedMaterial;

        /// <summary>
        /// Cache the flicker coroutine
        /// </summary>
        Coroutine _flickerCoroutine;

        /// <summary>
        /// Cache the flash coroutine
        /// </summary>
        Coroutine _flashCoroutine;

        private void Start()
        {
            //Save original material
            _originalSharedMaterial = gameObject.GetComponent<Renderer>().sharedMaterial;
        }

        private void OnDestroy()
        {
            //Instanced materials are not managed by Unity
            //Make sure to destroy the material
            Destroy(_instancedMaterial);
        }

        /// <summary>
        /// Plays a flicker effect on the gameobject
        /// </summary>
        /// <param name="duration">How long flickering will last</param>
        /// <param name="rate">Time between switching states</param>
        public void PlayFlicker(float duration = 1.5f, float rate = 0.1f)
        {
            //Only one flicker routine should be running
            if (_flickerCoroutine != null) return;

            _flickerCoroutine = StartCoroutine(FlickerRoutine(duration, rate));
        }

        /// <summary>
        /// Stops the flickering effect manually
        /// </summary>
        public void StopFlicker()
        {
            if (_flickerCoroutine == null) return;

            //Make sure object is visible
            if(gameObject.TryGetComponent<Renderer>(out Renderer r))
            {
                r.enabled = true;
            }

            StopCoroutine(_flickerCoroutine);
            _flickerCoroutine = null;
        }

        /// <summary>
        /// Plays the colour flash effect
        /// </summary>
        /// <param name="colour">Colour to flash</param>
        /// <param name="duration">Duration of colour change</param>
        public void PlayFlashColour(Color colour, float duration = 0.1f)
        {
            //Only one flash routine should be running
            if (_flashCoroutine != null) return;

            _flashCoroutine = StartCoroutine(FlashRoutine(colour, duration));
        }

        /// <summary>
        /// Stops the colour flash effect manually
        /// </summary>
        public void StopFlashColour()
        {
            if (_flashCoroutine == null) return;

            //Reset material to original
            if(gameObject.TryGetComponent<Renderer>(out Renderer r))
            {
                r.material = _originalSharedMaterial;
            }
            //Make sure instanced material is destroyed
            Destroy(_instancedMaterial);

            StopCoroutine(_flashCoroutine);
            _flashCoroutine = null;
        }

        /// <summary>
        /// Coroutine for flickering this script's gameobject
        /// </summary>
        IEnumerator FlickerRoutine(float duration, float rate)
        {
            //Track time
            float elapsed = 0.0f;
            float flickerTimer = 0.0f;

            //Cache renderer component
            Renderer renderer = gameObject.GetComponent<Renderer>();
            //Skip flickering if there is no renderer
            if (renderer == null)
            {
                _flickerCoroutine = null;
                yield break;
            }

            //Flicker
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                flickerTimer += Time.deltaTime;

                if (flickerTimer >= rate)
                {
                    renderer.enabled = !renderer.enabled;
                    flickerTimer = 0.0f;
                }

                yield return null;
            }

            renderer.enabled = true;
            _flickerCoroutine = null;
        }

        /// <summary>
        /// Coroutine for changing the colour of this object for a duration
        /// </summary>
        IEnumerator FlashRoutine(Color colour, float duration)
        {
            Renderer renderer = gameObject.GetComponent<Renderer>();
            //Skip flashing if there is no renderer
            if (renderer == null)
            {
                _flashCoroutine = null;
                yield break;
            }

            //Clone the material and start using it from now on
            _instancedMaterial = renderer.material;

            //Set emission and colour on instanced material
            _instancedMaterial.EnableKeyword("_EMISSION");
            _instancedMaterial.SetColor("_EmissionColor", colour);
            _instancedMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;

            //Wait before resetting material
            yield return new WaitForSeconds(duration);

            //Reset material to original
            renderer.material = _originalSharedMaterial;
            Destroy(_instancedMaterial);

            _flashCoroutine = null;
        }
    }
}
