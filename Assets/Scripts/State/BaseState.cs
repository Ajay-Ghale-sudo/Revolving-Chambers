using System.Collections.Generic;
using System;

namespace State
{
    /// <summary>
    /// Generic state implementation. 
    /// </summary>
    /// <typeparam name="T">Owner type.</typeparam>
    public abstract class BaseState<T> : BaseState
    {
        /// <summary>
        /// Owner of the state.
        /// </summary>
        protected readonly T _owner;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">Owner of this state</param>
        public BaseState(T owner)
        {
            _owner = owner;
        }
    }
    
    /// <summary>
    /// Base state class.
    /// </summary>
    public abstract class BaseState : IState
    {
        public BaseState()
        {
            this.id = Guid.NewGuid();
        }

        /// <summary>
        /// Unique ID of this state instance.
        /// </summary>
        public Guid id { get; }

        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public virtual void OnEnter()
        {
        }

        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        public virtual void OnExit()
        {
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        public virtual void Update()
        {
        }

        /// <summary>
        /// Called every fixed frame.
        /// </summary>
        public virtual void FixedUpdate()
        {
        }
    }
    
    /// <summary>
    /// A phase of a Boss phase. Has multiple concurrent states.
    /// </summary>
    public class BossPhase : BaseState
    {
        /// <summary>
        /// Concurrent states in this phase.
        /// </summary>
        private List<IState> _states = new();
        
        public BossPhase()
        {
            
        }
        
        public override void OnEnter()
        {
            _states.ForEach(state => state.OnEnter());
        }

        public override void OnExit()
        {
            _states.ForEach(state => state.OnExit());
        }

        public override void Update()
        {
            _states.ForEach(state => state.Update());
        }

        public override void FixedUpdate()
        {
            _states.ForEach(state => state.FixedUpdate());
        }
        
        public void AddState(IState state)
        {
            _states.Add(state);
        }
        
        public void RemoveState(IState state)
        {
            _states.Remove(state);
        }
    }
}