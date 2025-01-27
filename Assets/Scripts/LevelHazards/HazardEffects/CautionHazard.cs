using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using Interfaces;
using DG.Tweening;

namespace LevelHazards
{
    public class CautionHazard : MonoBehaviour
    {
        /// <summary>
        /// Event that is invoked when the attack is completed
        /// </summary>
        public UnityEvent OnLaunch;

        /// <summary>
        /// Event that is invoked when the attack is completed
        /// </summary>
        public UnityEvent OnFinished;

        [Header("References")]
        /// <summary>
        /// Mesh of the UV we want to scroll.
        /// </summary>
        [Tooltip("Mesh of the UV we want to scroll.")]
        [SerializeField] private MeshFilter _barrierMesh;

        /// <summary>
        /// Particle system to control.
        /// </summary>
        [SerializeField] private ParticleSystem _particleSystem;

        [Header("Variables")]
        /// <summary>
        /// Damage data for the hazard.
        /// </summary>
        [SerializeField] private DamageData _damageData;

        /// <summary>
        /// The duration of this effect.
        /// </summary>
        [SerializeField] private float _effectDuration = 1.0f;

        /// <summary>
        /// Speed to scroll the UV. 0.1 = 10% of image
        /// </summary>
        [Range(0.01f, 2.0f)]
        public float ScrollSpeed = 0.5f;

        /// <summary>
        /// The collider component attached to this gameObject
        /// </summary>
        private Collider _collider;

        /// <summary>
        /// Instanced mesh data. Not handled by Unity; manual cleanup required.
        /// </summary>
        private Mesh _instancedMesh;

        /// <summary>
        /// The UV data that will be moved by script
        /// </summary>
        private Vector2[] _originalUVs;

        /// <summary>
        /// The UV data that will be moved by script
        /// </summary>
        private Vector2[] _movingUVs;

        /// <summary>
        /// Instanced material data. Not handled by Unity; manual cleanup required.
        /// </summary>
        private Material _instancedMaterial;

        /// <summary>
        /// Current running coroutine
        /// </summary>
        private Coroutine _currentBehaviour;

        public void Start()
        {
            _instancedMesh = null;
            _collider = GetComponent<Collider>();

            if (_barrierMesh == null) return;

            //Cache the mesh component to set it's UV later
            _instancedMesh = _barrierMesh.mesh;

            //Cache the UV data for moving later
            _originalUVs = _barrierMesh.mesh.uv;
            _movingUVs = _barrierMesh.mesh.uv;

        }

        public void OnDestroy()
        {
            Destroy(_instancedMesh);
            Destroy(_instancedMaterial);
        }

        public void Launch()
        {
            //Enable hitbox collider
            if(_collider != null) { _collider.enabled = true; }

            //Enable particle system
            if (_particleSystem != null) { _particleSystem.gameObject.SetActive(true); }

            //Start the effect tiny and scale to normal size
            gameObject.transform.localScale = Vector3.zero;
            gameObject.transform.DOScale(Vector3.one, 0.5f);

            //Start scrolling mesh UVs
            if (_currentBehaviour == null) 
            { 
                _barrierMesh.gameObject.SetActive(true);
                _currentBehaviour = StartCoroutine(ActiveHazardRoutine()); 
            }
        }

        /// <summary>
        /// Coroutine that runs while this hazard is active.
        /// keeps track of time and scrolls UVs.
        /// </summary>
        /// <returns></returns>
        IEnumerator ActiveHazardRoutine()
        {
            float elapsed = 0.0f;
            float currentUvOffset = 0.0f;

            while (elapsed < _effectDuration)
            {
                elapsed += Time.deltaTime;
                currentUvOffset = Mathf.Repeat(currentUvOffset + (ScrollSpeed * Time.deltaTime), 1.0f);

                //Adjust moving UV positions
                for (int i = 0; i < _movingUVs.Length; i++)
                {
                    _movingUVs[i] = new Vector2(_originalUVs[i].x + currentUvOffset, _originalUVs[i].y);
                }

                //Apply new UV data to instanced mesh
                _instancedMesh.uv = _movingUVs;

                yield return null;
            }

            _currentBehaviour = null;

            //Scale to 0 and then call OnFinish
            gameObject.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() => { OnFinished?.Invoke(); });
        }

        /// <summary>
        /// Disable the effect manually without calling OnFinish. Will not be destroyed
        /// </summary>
        public void DisableHazard()
        {
            //Disable hitbox collider
            if (_collider != null) { _collider.enabled = false; }

            //Disable particle system
            if (_particleSystem != null) { _particleSystem.gameObject.SetActive(false); }

            //Stop any running coroutines
            if (_currentBehaviour != null)
            {
                _barrierMesh.gameObject.SetActive(false);
                StopCoroutine(_currentBehaviour);
                _currentBehaviour = null;
            }
        }

        /// <summary>
        /// Detect player Damageable component and deal damage if found.
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (other.TryGetComponent(out Damageable script))
                {
                    script.TakeDamage(_damageData);
                }
            }
        }
    }
}
