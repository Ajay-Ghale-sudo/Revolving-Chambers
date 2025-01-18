namespace State
{
    /// <summary>
    /// Interface for a transition.
    /// </summary>
    public interface ITransition
    {
        /// <summary>
        /// The state to transition to.
        /// </summary>
        IState To { get; }
        
        /// <summary>
        /// The condition to evaluate. Will transition if true.
        /// </summary>
        IPredicate Condition { get; }
    }

}