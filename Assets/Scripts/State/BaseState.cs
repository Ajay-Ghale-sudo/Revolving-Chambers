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
            
        }
        
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
}