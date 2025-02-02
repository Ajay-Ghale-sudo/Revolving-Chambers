using System;
using Audio;
using Events;
using Interfaces;
using State;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Weapon;

namespace Player
{
    /// <summary>
    /// Settings for the dash ability.
    /// </summary>
    [Serializable]
    public struct DashSettings
    {
        /// <summary>
        /// The duration of the dash.
        /// </summary>
        public float duration;
        
        /// <summary>
        /// The velocity of the dash.
        /// </summary>
        public float velocity;
        
        /// <summary>
        /// The cooldown of the dash.
        /// </summary>
        public float cooldown;
        
        /// <summary>
        /// The curve for the dash.
        /// </summary>
        public AnimationCurve curve;
    }
    
    /// <summary>
    /// Player movement.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMovement : Damageable
    {
        /// <summary>
        /// Event invoked when the dash starts.
        /// </summary>
        public UnityEvent OnDashStart;
        
        /// <summary>
        /// Event invoked when the dash ends.
        /// </summary>
        public UnityEvent OnDashEnd;
        
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
        /// The script for handling character animator controls
        /// </summary>
        [SerializeField] private CharacterAnimationHandler _animationHandler;

        /// <summary>
        /// The capsule collider.
        /// </summary>
        private CapsuleCollider _capsule;

        /// <summary>
        /// The speed at which the player moves.
        /// </summary>
        [SerializeField] private float MoveSpeed = 15f;

        /// <summary>
        /// Health of the player. Represented in hits they can take.
        /// </summary>
        [SerializeField] private new int Health = 5;
        
        /// <summary>
        /// Max health of the player. Represented in hits they can take.
        /// </summary>
        [SerializeField] private new int MaxHealth = 5;

        /// <summary>
        /// Health restored on revive.
        /// </summary>
        [SerializeField] private int healthRestoredOnRevive = 3;

        /// <summary>
        /// Settings for the dash ability.
        /// </summary>
        [SerializeField] private DashSettings dashSettings = new()
        {
            duration = 0.2f,
            velocity = 20f,
            cooldown = 3f
        };
        
        /// <summary>
        /// The coroutine for the dash.
        /// </summary>
        private Coroutine _dashCoroutine;
        
        /// <summary>
        /// If the player is dashing.
        /// </summary>
        private bool _isDashing = false;
        
        /// <summary>
        /// If the player can dash.
        /// </summary>
        private bool _canDash = true;

        /// <summary>
        /// Event invoked when the player dashes.
        /// </summary>
        [SerializeField] public UnityEvent OnDashEvent;
        
        /// <summary>
        /// The weapon the player is using.
        /// </summary>
        private IWeapon _weapon;

        /// <summary>
        /// The character controller.
        /// </summary>
        private CharacterController _characterController;

        /// <summary>
        /// If the player is currently dying.
        /// </summary>
        private bool _isDying = false;

        /// <summary>
        /// If the player is currently paused.
        /// </summary>
        private bool _isPaused = false;

        /// <summary>
        /// Audio event to fire when the player dashes.
        /// </summary>
        public AudioEvent _dashAudioEvent;
        
        /// <summary>
        /// Audio event to fire when the player takes damage.
        /// </summary>
        public AudioEvent _takeDamageAudioEvent;

        /// <summary>
        /// Grace time for taking damage.
        /// </summary>
        [SerializeField]
        private float damageGraceTime = .5f;
        
        /// <summary>
        /// Whether to lock the y position of the player.
        /// </summary>
        [SerializeField]
        private bool lockYPosition = true;
        
        /// <summary>
        /// The starting y position of the player.
        /// </summary>
        private float _yPosition;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _mainCamera ??= GetComponent<Camera>();
            _rb = GetComponent<Rigidbody>();
            _capsule = GetComponentInChildren<CapsuleCollider>();
            _characterController = GetComponent<CharacterController>();
            
            GameStateManager.Instance.OnPlayerRevive += Revive;
            GameStateManager.Instance.OnGamePause += OnGamePause;
            GameStateManager.Instance.OnPlayerHeal += Heal;
            GameStateManager.Instance.OnBossDeath += DisableDamage;
            
            _yPosition = transform.position.y;

            // TEMP FOR TESTING
            _weapon = GetComponentInChildren<IWeapon>();
            
            // Lock cursor to window
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = false;
        }

        private void OnDestroy()
        {
            GameStateManager.Instance.OnPlayerRevive -= Revive;
            GameStateManager.Instance.OnGamePause -= OnGamePause;
            GameStateManager.Instance.OnPlayerHeal -= Heal;
            GameStateManager.Instance.OnBossDeath -= DisableDamage;
        }

        // Update is called once per frame
        void Update()
        {
            LookTowardsCursor();
            Move();
            if(!_isPaused) UpdateAnimations();
        }

        /// <summary>
        /// Move the player.
        /// </summary>
        void Move()
        {
            if (_isDying || _isPaused) return;
            // Move the player
            _characterController.Move(_moveVelocity * Time.deltaTime);

            if (lockYPosition)
            {
                transform.position = new Vector3(transform.position.x, _yPosition, transform.position.z);
            }
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
            if (_isDashing || _isDying || _isPaused) return;
            _weapon?.Fire();
        }

        /// <summary>
        /// Handle the dash input.
        /// </summary>
        void OnDash()
        {
            if (_isPaused) return;
            Dash();
        }

        /// <summary>
        /// Dash the player.
        /// </summary>
        void Dash()
        {
            if (_dashCoroutine != null) return;
            if (!_canDash) return;
            _dashCoroutine = StartCoroutine(DashRoutine());
        }
        
        /// <summary>
        /// Enable the dash ability.
        /// </summary>
        private void EnableDash()
        {
            _canDash = true;
        }
        
        /// <summary>
        /// Coroutine for the dash ability.
        /// </summary>
        /// <returns></returns>
        private System.Collections.IEnumerator DashRoutine()
        {
            float time = 0;
            float startTime = Time.time;
            _canDash = false;
            _isDashing = true;

            _dashAudioEvent?.Invoke();
            
            OnDashStart?.Invoke();
            
            // Dash direction based on last movement input, if 0 then dash forward
            var dashDirection = _moveInput == Vector3.zero ? transform.forward : _moveInput;
            while (time < dashSettings.duration)
            {
                time = Time.time - startTime;
                float t = time / dashSettings.duration;
                float curveValue = dashSettings.curve.Evaluate(t);
                _characterController.Move(dashDirection * (dashSettings.velocity * curveValue * Time.deltaTime));
                yield return null;
            }

            _isDashing = false;
            _dashCoroutine = null;
            OnDashEnd?.Invoke();
            Invoke(nameof(EnableDash), dashSettings.cooldown);
        }
        
        /// <summary>
        ///  Handle the right click input.
        /// </summary>
        void OnRightClick()
        {
            if (!Input.GetMouseButtonDown(1)) return;
            if (_isPaused) return;
            UIManager.Instance.OnSpinEnd?.Invoke();
        }
        
        /// <summary>
        /// Handle the reload input.
        /// </summary>
        void OnReload()
        {
            if (_isPaused) return;
            _weapon?.Reload();
        }

        /// <summary>
        /// Handle the pause input.
        /// </summary>
        void OnPause()
        {
            UIManager.Instance.OnTogglePauseScreen?.Invoke();
        }

        void OnPoint(InputValue value)
        {
            UIManager.Instance.OnPointerMove?.Invoke(value.Get<Vector2>());
        }

        /// <summary>
        /// Do stuff when the game is paused/unpaused
        /// </summary>
        /// <param name="state">pause/unpause</param>
        void OnGamePause(bool state)
        {
            _isPaused = state;
        }

        /// <summary>
        /// Rotate player to look towards the cursor.
        /// </summary>
        private void LookTowardsCursor()
        {
            if (_isDying || _isPaused) return;
            // Get cursor 
            Vector3 mousePos = Mouse.current.position.ReadValue();
            Ray ray = _mainCamera.ScreenPointToRay(mousePos);
            Vector3 targetPos;
            if (!Physics.Raycast(ray, out RaycastHit hit))
            {
                // Fallback to ground level if no hit
                var groundPlane = new Plane(Vector3.up, Vector3.zero);
                groundPlane.Raycast(ray, out float distance);
                hit.point = ray.GetPoint(distance);
                
            }
            
            targetPos = new Vector3(hit.point.x, _capsule.transform.position.y, hit.point.z);
            // Aim towards hit point X and Z
            _capsule.transform.LookAt(targetPos);
        }

        private void UpdateAnimations()
        {
            if (_capsule == null) return;

            //Current facing direction normalized
            Vector2 facingDir = new Vector2(_capsule.transform.forward.x, _capsule.transform.forward.z);
            //Last movement input direction normalized
            Vector2 targetDir = new Vector2(_moveInput.x, _moveInput.z).normalized;

            //We need to find the movement direction relative to our facing direction.
            //Example: facing North + moving South = backwards animation
            //         facing South + moving South = forwards animation
            //         facing East + moving South = strafe right animation

            //Find the angle that will orient our facingDir with Forward animation property (0, 1)
            float angle = Vector2.SignedAngle(facingDir, new Vector2(0.0f, 1.0f)) * Mathf.Deg2Rad;

            //Rotate targetDir by angle amount to get our direction vector for animator
            Vector2 mappedDir = new Vector2(
                (targetDir.x * Mathf.Cos(angle)) - (targetDir.y * Mathf.Sin(angle)),
                (targetDir.x * Mathf.Sin(angle)) + (targetDir.y * Mathf.Cos(angle)));

            //Set animator values
            _animationHandler?.SetForward(mappedDir.y);
            _animationHandler?.SetRight(mappedDir.x);
        }

        /// <summary>
        /// Revive the player.
        /// </summary>
        private void Revive()
        {
            if (!_isDying) return;
            _isDying = false;
            Health = healthRestoredOnRevive;
            _animationHandler?.Play_Revive();
            UIManager.Instance.OnPlayerHealthChange?.Invoke(Health);
        }
        
        private void Heal(int amount)
        {
            Health = Mathf.Clamp(Health + amount, 0, MaxHealth);
            UIManager.Instance.OnPlayerHealthChange?.Invoke(Health);
        }

        /// <summary>
        /// Kill the player.
        /// </summary>
        protected override void Die()
        {
            if (_isDying) return;
            _isDying = true;
            OnDeath?.Invoke();
            _animationHandler?.Play_Death();
            GameStateManager.Instance.OnPlayerDeath?.Invoke();
        }

        // IDamageable implementation \\
        public override void TakeDamage(DamageData damage)
        {
            if (_isDashing || !_damageEnabled) return;
            
            DisableDamage();
            Invoke(nameof(EnableDamage), damageGraceTime);
            
            Health -= damage.damage;
            OnDamage?.Invoke();
            _takeDamageAudioEvent?.Invoke();
            // TODO: This likely doesn't need a direct reference to UI Manager. Low priority cleanup for later.
            UIManager.Instance.OnPlayerHealthChange?.Invoke(Health);
            if (Health <= 0) Die();
        }

        public override void PlayDamageEffect(Color colour)
        {
            // Move to event
        }
        
        // End IDamageable implementation \\
    }
}