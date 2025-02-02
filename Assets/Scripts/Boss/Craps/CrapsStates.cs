using DG.Tweening;
using State;
using UnityEngine;

namespace Boss.Craps
{
    /// <summary>
    /// Craps Boss Idle State
    /// </summary>
    public class CrapsIdleState : BaseState<CrapsBoss>
    {
        public CrapsIdleState(CrapsBoss owner) : base(owner)
        {
            
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }

    /// <summary>
    /// State for the diamond boss to roll to the side.
    /// </summary>
    public class SideRollingState : BaseState<CrapsBoss>
    {
        /// <summary>
        /// Direction to move the boss.
        /// </summary>
        enum Direction
        {
            Left,
            Center,
            Right
        }
        
        /// <summary>
        /// Amount to move the boss.
        /// </summary>
        private float _moveAmount = 10f;
        
        /// <summary>
        /// Start position of the boss.
        /// </summary>
        private Vector3 _startPosition;
        
        /// <summary>
        /// Direction the boss is currently moving.
        /// </summary>
        private Direction _direction = Direction.Center;
        
        /// <summary>
        /// Direction the boss will move next.
        /// </summary>
        private Direction _nextDirection = Direction.Center;
        
        /// <summary>
        /// Tween to move the boss.
        /// </summary>
        private Tweener _moveTween;
        
        /// <summary>
        /// Tween to rotate the boss.
        /// </summary>
        private Tweener _rotateTween;
        
        /// <summary>
        /// Elapsed time since the boss started moving.
        /// </summary>
        private float _elapsedTime = 0f;
        
        /// <summary>
        /// Time to wait before the boss moves again.
        /// </summary>
        private float _timeToNextMove = 2f;
        
        /// <summary>
        /// Whether the boss is currently moving.
        /// </summary>
        private bool _isMoving = false;
        
        /// <summary>
        /// Duration of the move.
        /// </summary>
        private float _duration = 1f;
        
        /// <summary>
        /// Amount to rotate the boss.
        /// </summary>
        private float _rotateAmount = 90f;
        
        /// <summary>
        /// Amount to rotate the boss on the x axis.
        /// </summary>
        private float _rotateXAmount = 0;
        
        public SideRollingState(CrapsBoss owner) : base(owner)
        {
            
        }
        
        public override void OnEnter()
        {
            _elapsedTime = 0f;
            _startPosition = _owner.transform.position;
            MoveLeft();
        }
        
        public override void OnExit()
        {
            base.OnExit();
        }
        
        public override void Update()
        {
            _elapsedTime += Time.deltaTime;
            if (!_isMoving && _elapsedTime >= _timeToNextMove)
            {
                Move(_nextDirection);
            }
        }
        
        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        private void MoveComplete()
        {
            _owner.Shoot();
            _isMoving = false;
            _elapsedTime = 0f;
            _timeToNextMove = 2f;
            _nextDirection = _direction switch
            {
                Direction.Left => Direction.Center,
                Direction.Right => Direction.Center,
                Direction.Center => Random.value > 0.5f ? Direction.Left : Direction.Right,
                _ => _nextDirection
            };
        }
        
        private void Move(Direction direction)
        {
            _isMoving = true;
            switch (direction)
            {
                case Direction.Left:
                    MoveLeft();
                    break;
                case Direction.Right:
                    MoveRight();
                    break;
                case Direction.Center:
                    MoveBack();
                    break;
            }

            _owner?.MoveAudioEvent?.Invoke();
        }

        private void MoveLeft()
        {
            _direction = Direction.Left;
            _moveTween = _owner.transform.DOMoveX(_startPosition.x - _moveAmount, _duration).OnComplete(MoveComplete).SetEase(Ease.Linear).OnUpdate(OnUpdate);
            _rotateTween = _owner.transform.DORotateQuaternion(Quaternion.Euler(_rotateXAmount, 0, _rotateAmount), _duration).SetEase(Ease.Linear);
        }
        
        private void MoveRight()
        {
            _direction = Direction.Right;
            _moveTween = _owner.transform.DOMoveX(_startPosition.x + _moveAmount, _duration).OnComplete(MoveComplete).SetEase(Ease.Linear);
            _rotateTween = _owner.transform.DORotateQuaternion(Quaternion.Euler(_rotateXAmount, 0, -_rotateAmount), _duration).SetEase(Ease.Linear);
        }
        
        
        private void MoveBack()
        {
            _direction = Direction.Center;
            _moveTween = _owner.transform.DOMoveX(_startPosition.x, _duration).SetEase(Ease.Linear).OnComplete(MoveComplete);
            _rotateTween = _owner.transform.DORotateQuaternion(Quaternion.Euler(_rotateXAmount, 0, 0), _duration).SetEase(Ease.Linear);
        }

        private void OnUpdate()
        {
            
        }
    }
    
    public class CrapsDeadState : BaseState<CrapsBoss>
    {
        public CrapsDeadState(CrapsBoss owner) : base(owner)
        {
            
        }
        
        public override void OnEnter()
        {
            base.OnEnter();
        }
        
        public override void OnExit()
        {
            base.OnExit();
        }
        
        public override void Update()
        {
            base.Update();
        }
        
        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
    }
}