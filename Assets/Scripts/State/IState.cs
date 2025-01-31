using UnityEditor;

namespace State
{
    /// <summary>
    /// Interface for a state.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// The id of the state.
        /// </summary>
        GUID id { get; }
        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        void OnEnter();
        
        /// <summary>
        /// Called when the state is exited.
        /// </summary>
        void OnExit();
        
        /// <summary>
        /// Called every frame.
        /// </summary>
        void Update();
        
        /// <summary>
        /// Called every fixed frame.
        /// </summary>
        void FixedUpdate();
    }

    
}