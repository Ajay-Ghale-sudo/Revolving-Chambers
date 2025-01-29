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
        /// Instanced material that should be destroyed manually.
        /// Used for changing this object's material through script.
        /// </summary>
        Material _instancedMaterial;

        /// <summary>
        /// The default colour of emission (when resetting)
        /// </summary>
        Color _defaultColor;

        /// <summary>
        /// The renderer for this gameobject
        /// </summary>
        Renderer _renderer;

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
            if (gameObject.TryGetComponent(out Renderer rend))
            {
                //Cache renderer
                _renderer = rend;

                //Create + cache instanced material
                //Clone the material and start using it from now on
                _instancedMaterial = rend.material;

                //Turn on emission but set it to black (basically no emission)
                _instancedMaterial.EnableKeyword("_EMISSION");
                _instancedMaterial.SetColor("_EmissionColor", Color.black);
                _instancedMaterial.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            }

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

            ResetColor();

            StopCoroutine(_flashCoroutine);
            _flashCoroutine = null;
        }

        /// <summary>
        /// Sets the default the emission colour of the material. 
        /// Can set to black to turn off emission effect
        /// </summary>
        /// <param name="color">Colour to change to</param>
        public void SetColor(Color color)
        {
            _defaultColor = color;
            _instancedMaterial.SetColor("_EmissionColor", color);
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
            //Skip flashing if there is no renderer
            if (_renderer == null)
            {
                _flashCoroutine = null;
                yield break;
            }

            //Set emission colour on instanced material
            _instancedMaterial.SetColor("_EmissionColor", colour);

            //Wait before resetting material
            yield return new WaitForSeconds(duration);

            ResetColor();

            _flashCoroutine = null;
        }

        /// <summary>
        /// Resets to the default colour
        /// </summary>
        public void ResetColor()
        {
            if (_renderer == null) return;

            _instancedMaterial.SetColor("_EmissionColor", _defaultColor);
        }

    }
}
