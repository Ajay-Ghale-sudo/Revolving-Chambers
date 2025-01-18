using System;

namespace State
{
    /// <summary>
    /// Predicate that uses a function to evaluate.
    /// </summary>
    public class FuncPredicate : IPredicate
    {
        /// <summary>
        /// The function to evaluate.
        /// </summary>
        readonly Func<bool> _func;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="func">Function to use in the evaluate condition.</param>
        public FuncPredicate(Func<bool> func)
        {
            _func = func;
        }

        /// <summary>
        /// Evaluate the function.
        /// </summary>
        /// <returns>Result of the condition function.</returns>
        public bool Evaluate() => _func.Invoke();
    }
}