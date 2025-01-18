using System;

namespace State
{
    /// <summary>
    /// Interface for a predicate.
    /// </summary>
    public interface IPredicate
    {
        /// <summary>
        /// Evaluate the condition.
        /// </summary>
        /// <returns>True, if if the condition is met.</returns>
        bool Evaluate();
    }

}