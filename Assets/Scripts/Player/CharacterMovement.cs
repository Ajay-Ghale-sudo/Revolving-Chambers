using Interfaces;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Weapon;

namespace Player
{
    /// <summary>
    /// Player movement.
    /// </summary>
    public class CharacterMovement : MonoBehaviour, IDamageable
    {
        /// <summary>
        /// The movement input vector.
        /// </summary>
        private Vector3 _moveInput;
        
        /// <summary>
        /// The movement velocity.
        /// </summary>
        private Vector3 _moveVelocity;
        
        /// <summary>
        /// The rigidbody component.
        /// </summary>
        private Rigidbody _rb;

        /// <summary>
        /// The camera being used by the player.
        /// </summary>
        [SerializeField] private Camera _mainCamera;

        /// <summary>
        /// The capsule collider.
        /// </summary>
        private CapsuleCollider _capsule;

        /// <summary>
        /// The speed at which the player moves.
        /// </summary>
        [SerializeField] private float MoveSpeed = 15f;

        /// <summary>
        /// Health of the player.
        /// </summary>
        [SerializeField] private float Health = 100f;
        
        private IWeapon _weapon;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _mainCamera ??= GetComponent<Camera>();
            _rb = GetComponent<Rigidbody>();
            _capsule = GetComponentInChildren<CapsuleCollider>();
            
            // TEMP FOR TESTING
            _weapon = GetComponentInChildren<IWeapon>();
        }

        // Update is called once per frame
        void Update()
        {
            LookTowardsCursor();
            Move();
        }

        /// <summary>
        /// Move the player.
        /// </summary>
        void Move()
        {
            // Move the player
            transform.parent.Translate(_moveVelocity * Time.deltaTime, Space.World);
        }

        /// <summary>
        /// Handle the move input.
        /// </summary>
        /// <param name="value">InputValue of the move action</param>
        void OnMove(InputValue value)
        {
            Vector2 inputVector = value.Get<Vector2>();
            _moveInput = new Vector3(inputVector.x, 0, inputVector.y);
            _moveInput = _moveInput.normalized;
            _moveVelocity = _moveInput * MoveSpeed;
        }

        /// <summary>
        ///  Handle the attack input.
        /// </summary>
        void OnAttack()
        {
            _weapon?.Fire();
        }
        
        /// <summary>
        ///  Handle the right click input.
        /// </summary>
        void OnRightClick()
        {
            ReloadManager.Instance.OnSpinEnd?.Invoke();
        }
        
        /// <summary>
        /// Handle the reload input.
        /// </summary>
        void OnReload()
        {
            _weapon?.Reload();
        }

        /// <summary>
        /// Rotate player to look towards the cursor.
        /// </summary>
        private void LookTowardsCursor()
        {
            // Get cursor 
            Vector3 mousePos = Mouse.current.position.ReadValue();
            Ray ray = _mainCamera.ScreenPointToRay(mousePos);
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayDistance;
            if (groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 point = ray.GetPoint(rayDistance);
                Debug.DrawLine(ray.origin, point, Color.red);
                _capsule.transform.LookAt(new Vector3(point.x, _capsule.transform.position.y, point.z));
            }
        }

        // IDamageable implementation \\
        public UnityEvent OnDeath { get; set; }
        public UnityEvent OnDamage { get; set; }
        public void TakeDamage(DamageData damage)
        {
            Health -= damage.damage;
            OnDamage?.Invoke();
            if (Health > 0) return;
            OnDeath?.Invoke();
        }

        public void PlayDamageEffect(Color colour)
        {
            // Move to event
        }
        
        // End IDamageable implementation \\
    }
}