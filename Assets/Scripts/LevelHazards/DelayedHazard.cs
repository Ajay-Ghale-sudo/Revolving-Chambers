using UnityEngine;
using UnityEngine.Events;
using Interfaces;
using System.Collections;
using DG.Tweening;

namespace LevelHazards
{
    /// <summary>
    /// Control script for environment hazard. Can be used by scripts:
    /// 
    /// DelayedHazard hazardScript = Instantiate(HazardPrefab).GetComponent<DelayedHazard>();
    /// if(hazardScript != null) hazardScript.Launch_Follow(Delay, Target.transform);
    /// 
    /// </summary>
    public class DelayedHazard : MonoBehaviour
    {
        /// <summary>
        /// Event invoked when the hazard is activated.
        /// </summary>
        public UnityEvent OnHazardActivated;

        /// <summary>
        /// Speed at which the hazard moves.
        /// </summary>
        [SerializeField] private float _moveSpeed = 7.5f;

        /// <summary>
        /// The time it takes to reach full opacity from launch
        /// </summary>
        [SerializeField] private float _fadeInTime = 0.1f;

        /// <summary>
        /// The delay between reaching target and activating
        /// </summary>
        [SerializeField] private float _activationDelay = 0.1f;

        /// <summary>
        /// Change to this colour before activating
        /// </summary>
        [SerializeField] private Color _activationColor;

        /// <summary>
        /// Launch this hazard on start
        /// </summary>
        [Tooltip("Will start the follow player behaviour")]
        [SerializeField] private bool _launchOnStart = true;

        /// <summary>
        /// Time to activate the hazard.
        /// </summary>
        [Tooltip("Time delay before activating. Only works when launching on start.")]
        [SerializeField] private float _followTime = 1.5f;

        /// <summary>
        /// Instanced material data. Not handled by Unity; manual cleanup required.
        /// </summary>
        private Material _instancedMaterial;

        /// <summary>
        /// Current behaviour that is running. (Tracking or set destination)
        /// </summary>
        private Coroutine _behaviourRoutine;

        public void Start()
        {
            if(gameObject.TryGetComponent(out Renderer renderer))
            {
                _instancedMaterial = renderer.material;
            }

            if (_launchOnStart)
            {
                Transform player = GameObject.FindGameObjectWithTag("Player").transform;
                if (player != null)
                {
                    Launch_Follow(_followTime, player);
                    //Launch_Destination(new Vector3(5.0f, 0.0f, 2.0f));
                }
            }
        }

        /// <summary>
        /// Follow a transform and activate after a short time.
        /// Move at default speed.
        /// </summary>
        /// <param name="delay">Delay before activating</param>
        /// <param name="target">Transform to follow</param>
        public void Launch_Follow(float delay, Transform target) { Launch_Follow(delay, target, _moveSpeed); }

        /// <summary>
        /// Follow a transform and activate after a short time.
        /// Move at custom speed.
        /// </summary>
        /// <param name="delay">Delay before activating</param>
        /// <param name="target">Transform to follow</param>
        /// <param name="speed">Move speed</param>
        public void Launch_Follow(float delay, Transform target, float speed)
        {
            _moveSpeed = speed;

            Color32 opacity = new Color32(0, 0, 0, 150);
            _instancedMaterial.DOColor(opacity, _fadeInTime);

            StopCurrentAction();

            _behaviourRoutine = StartCoroutine(TrackingRoutine(delay, target));
        }

        /// <summary>
        /// Move towards a position and activate when reached.
        /// Move at default speed.
        /// </summary>
        /// <param name="position">Position to move to</param>
        public void Launch_Destination(Vector3 position) { Launch_Destination(position, _moveSpeed); }

        /// <summary>
        /// Move towards a position and activate when reached.
        /// Move at custon speed.
        /// </summary>
        /// <param name="position">Position to move to</param>
        /// <param name="speed">Move speed</param>
        public void Launch_Destination(Vector3 position, float speed)
        {
            _moveSpeed = speed;

            Color32 opacity = new Color32(0, 0, 0, 150);
            _instancedMaterial.DOColor(opacity, _fadeInTime);

            StopCurrentAction();

            _behaviourRoutine = StartCoroutine(ActivateOnPositionRoutine(position));
        }

        /// <summary>
        /// Stops any behaviour coroutines
        /// </summary>
        public void StopCurrentAction()
        {
            if (_behaviourRoutine == null) return;
            
            StopCoroutine(_behaviourRoutine);
            _behaviourRoutine = null;
        }

        /// <summary>
        /// Coroutine that continuously follows a transform. Activates after a period of time
        /// </summary>
        /// <param name="delay">Time until activation</param>
        /// <param name="target">Transform to follow</param>
        IEnumerator TrackingRoutine(float delay, Transform target)
        {
            float timer = 0.0f;

            //Keep moving towards target until time delay
            while (timer < delay && target != null)
            {
                timer += Time.deltaTime;

                transform.position = Vector3.MoveTowards(transform.position, target.position, _moveSpeed * Time.deltaTime);
                yield return null;
            }

            //Reached the target/destination where we want to activate the hazard
            _instancedMaterial?.SetColor("_EmissionColor", _activationColor);

            //Reset timer and wait a short delay before activating the hazard
            timer = 0.0f;
            while (timer < _activationDelay)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            OnHazardActivated?.Invoke();
            _behaviourRoutine = null;
        }

        /// <summary>
        /// Coroutine that moves to a position. Activates when position is reached
        /// </summary>
        /// <param name="position">Position to move to</param>
        IEnumerator ActivateOnPositionRoutine(Vector3 position)
        {
            //Move towards the world position
            while (Vector3.Distance(position, transform.position) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, position, _moveSpeed * Time.deltaTime);
                yield return null;
            }

            //Reached the target/destination where we want to activate the hazard
            _instancedMaterial?.SetColor("_EmissionColor", _activationColor);

            //Wait a short delay before activating the hazard
            float timer = 0.0f;
            while (timer < _activationDelay)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            OnHazardActivated?.Invoke();
            _behaviourRoutine = null;
        }

        /// <summary>
        /// Destroy this hazard prefab
        /// </summary>
        public void DestroyHazard()
        {
            Destroy(_instancedMaterial);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            Destroy(_instancedMaterial);
        }
    }
}
