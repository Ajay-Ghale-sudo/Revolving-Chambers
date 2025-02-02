using UnityEngine;
using UnityEngine.Events;
using Weapon;
using Props;
using DG.Tweening;
using Interfaces;
using System.Collections;
using System.Collections.Generic;

namespace LevelHazards
{
    /// <summary>
    /// Behaviour for bullet shields.
    /// Bullet shields can absorb all but one ammo type.
    /// The ammo type which is accepted will disable the shield for a short time.
    /// Accepted ammo type changes to a random one once disabled
    /// </summary>
    public class BulletShield : MonoBehaviour
    {
        /// <summary>
        /// Event called when shield is disabled.
        /// </summary>
        public UnityEvent OnDisabled;

        /// <summary>
        /// Event called when shield is reset.
        /// </summary>
        public UnityEvent OnReset;

        /// <summary>
        /// Event called when shield blocks a bullet.
        /// </summary>
        public UnityEvent OnBlocked;

        /// <summary>
        /// Collider component used to detect bullet collision. Must be within child hierarchy.
        /// </summary>
        [Tooltip("Collider component used to detect bullet collision. Must be within child hierarchy.")]
        [SerializeField] private Collider _collider;

        /// <summary>
        /// Using the the DamageableVFX controller to set colours.
        /// </summary>
        [Tooltip("Using the the DamageableVFX controller to set colours.")]
        [SerializeField] private DamageableVFX _vfxController;

        /// <summary>
        /// Gameobject that acts as a pivot for the shield (playing card) mesh
        /// </summary>
        [Tooltip("Gameobject that acts as a pivot for the shield (playing card) mesh")]
        [SerializeField] private GameObject _pivot;

        /// <summary>
        /// List of potential ammo to absorb.
        /// </summary>
        [Tooltip("List of potential ammo to absorb.")]
        [SerializeField] private List<Ammo> _possibleAmmo;

        /// <summary>
        /// How long the shield will be disabled.
        /// </summary>
        [Tooltip("How long the shield will be disabled.")]
        [SerializeField] private float _disabledTime;

        /// <summary>
        /// The ammo type that can disable this shield.
        /// </summary>
        private Ammo _acceptedAmmo;

        /// <summary>
        /// Coroutine for timing the disabled state.
        /// </summary>
        private Coroutine _disabledRoutine;

        /// <summary>
        /// Prevents the shield from being disabled.
        /// </summary>
        private bool _impregnable = false;

        private void Start()
        {
            //Cache collider for disabling later
            _collider = GetComponent<Collider>();

            //Pick random ammo
            PickNewAmmo();

            //Set the colour to the new ammo
            SetCurrentColor();
        }

        public void OnCollisionEnter(Collision other)
        {
            if(!_impregnable && other.gameObject.TryGetComponent(out Bullet bullet))
            {
                //Disable emission for 0.2 seconds
                _vfxController?.PlayFlashColour(Color.black, 0.2f);

                if(bullet.Ammo != null && bullet.Ammo == _acceptedAmmo)
                {
                    DisableShield();
                }
            }
        }

        /// <summary>
        /// Sets the _impregnable flag.
        /// If true, the shield will not be disabled when hit
        /// </summary>
        /// <param name="state">State of flag</param>
        public void SetImpregnable(bool state)
        {
            _impregnable = state;

            SetCurrentColor();
        }

        /// <summary>
        /// Disables the shield and sets a new ammo type.
        /// </summary>
        public void DisableShield()
        {
            OnDisabled?.Invoke();

            //Turn off emission colour
            _vfxController?.SetColor(Color.black);

            //Randomly select a new ammo type
            PickNewAmmo();

            //Rotate card to flat position
            if (_pivot != null)
            {
                _pivot.gameObject.transform.DOLocalRotate(new Vector3(90.0f, 0.0f, 0.0f), 0.2f).OnComplete(() => { StartDisabledTimerRoutine(); });
            }

            //Disable collider
            if (_collider != null)
            {
                _collider.enabled = false;
            }
        }

        /// <summary>
        /// Resets the shield so it can block again
        /// </summary>
        public void ResetShield()
        {
            OnReset?.Invoke();

            //Cancel any coroutines that automate resetting
            StopDisabledTimerRoutine();

            //Set colour to current set ammo (ammo is randomized when disabled)
            SetCurrentColor();

            //Rotate card to standing position
            if (_pivot != null)
            {
                _pivot.gameObject.transform.DOLocalRotate(new Vector3(0.0f, 0.0f, 0.0f), 0.2f);
            }

            //Enable collider
            if (_collider != null)
            {
                _collider.enabled = true;
            }
        }

        /// <summary>
        /// Starts the coroutine that automatically resets the shield
        /// </summary>
        void StartDisabledTimerRoutine()
        {
            //Only one coroutine should be running
            if (_disabledRoutine != null) return;

            _disabledRoutine = StartCoroutine(AutoResetRoutine());
        }

        /// <summary>
        /// Manually stop the timer routine. 
        /// </summary>
        void StopDisabledTimerRoutine()
        {
            //Only one coroutine should be running
            if (_disabledRoutine != null) StopCoroutine(_disabledRoutine);

            _disabledRoutine = null;
        }

        /// <summary>
        /// Coroutine that waits for a duration and then resets the shield.
        /// </summary>
        IEnumerator AutoResetRoutine()
        {
            float timer = 0.0f;

            while (timer < _disabledTime)
            {
                timer += Time.deltaTime;

                yield return null;
            }

            ResetShield();
            _disabledRoutine = null;
        }

        /// <summary>
        /// Switch to the current colour (current ammo)
        /// or impregnable colour if set.
        /// </summary>
        void SetCurrentColor()
        {
            if (_impregnable)
            {
                //Change emission colour
                _vfxController?.SetColor(Color.black);
                _vfxController?.SetAlbedo(Color.black);
                return;
            }

            if (_acceptedAmmo != null)
            {
                _vfxController?.SetColor(_acceptedAmmo.color);
                _vfxController?.SetAlbedo(_acceptedAmmo.color);
            }
        }

        /// <summary>
        /// Select a random ammo type from the list
        /// </summary>
        void PickNewAmmo()
        {
            //Pick random ammo
            if (_possibleAmmo != null && _possibleAmmo.Count > 0)
            {
                _acceptedAmmo = _possibleAmmo[Random.Range(0, _possibleAmmo.Count)];
            }
        }
    }
}
