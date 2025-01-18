namespace State
{
    /// <summary>
    /// Transition between states.
    /// </summary>
    public class Transition : ITransition
    {
        /// <summary>
        /// The state to transition to.
        /// </summary>
        public IState To { get; }
        
        /// <summary>
        /// The condition to evaluate. Will transition if true.
        /// </summary>
        public IPredicate Condition { get; }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="to">State to transition to</param>
        /// <param name="condition">Condition to evaluate.</param>
        public Transition(IState to, IPredicate condition)
        {
            To = to;
            Condition = condition;
        }
    }
}