using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.XR;

namespace State
{
    /// <summary>
    /// Used to manage state for an object.
    /// </summary>
    public class StateMachine
    {
        /// <summary>
        /// The current state.
        /// </summary>
        private StateNode current;
        
        /// <summary>
        /// The previous state.
        /// </summary>
        private StateNode previous;
        
        /// <summary>
        /// The nodes for the state machine.
        /// </summary>
        private Dictionary<Guid, StateNode> nodes = new();
        
        /// <summary>
        /// The transitions that can be taken from any state.
        /// </summary>
        private HashSet<ITransition> anyTransitions = new();

        public void Update()
        {
            var transition = GetTransition();
            if (transition != null)
            {
                ChangeState(transition.To);
            }
            
            current?.State?.Update();
        }

        public void FixedUpdate()
        {
            current?.State?.FixedUpdate();
        }

        /// <summary>
        /// Set the state of the state machine.
        /// </summary>
        /// <param name="state">The State to update the machine to.</param>
        public void SetState(IState state)
        {
            if (current != null)
            {
                current.State?.OnExit();
                previous = current;
            }
            
            current = GetOrAddNode(state);
            current.State?.OnEnter();
        }

        /// <summary>
        /// Change the state of the state machine.
        /// </summary>
        /// <param name="state">The State to change to.</param>
        void ChangeState(IState state)
        {
            if (state == current.State) return;

            previous = current;
            var nextState = nodes[state.id];
            
            previous.State?.OnExit();
            nextState.State.OnEnter();
            current = nodes[state.id];
        }

        /// <summary>
        /// Get the transition to take.
        /// </summary>
        /// <returns>The first <see cref="ITransition"/> which conditions are met. Null if no viable transitions are found. </returns>
        ITransition GetTransition()
        {
            if (anyTransitions?.Count > 0)
                foreach (var transition in anyTransitions)
                {
                    if (transition.Condition.Evaluate())
                        return transition;
                }
            
            if (current?.Transitions?.Count > 0)
                foreach (var transition in current.Transitions)
                {
                    if (transition.Condition.Evaluate())
                        return transition;
                }

            return null;
        }
        
        /// <summary>
        /// Add a transition between two states.
        /// </summary>
        /// <param name="from">State to transition from.</param>
        /// <param name="to">State to transition to.</param>
        /// <param name="condition">The condition that must be met for this transition to happen.</param>
        public void AddTransition(IState from, IState to, IPredicate condition)
        {
            GetOrAddNode(from).AddTransition(GetOrAddNode(to).State, condition);
        }
        
        /// <summary>
        /// Add a transition to the current state.
        /// </summary>
        /// <param name="to">State to transition to.</param>
        /// <param name="condition">The condition that must be met for this transition to happen.</param>
        public void AddTransition(IState to, IPredicate condition)
        {
            current.Transitions.Add(new Transition(to, condition));
        }
        /// <summary>
        /// Add a transition that can be taken from any state.
        /// </summary>
        /// <param name="to">State to transition to.</param>
        /// <param name="condition">The condition that must be met for this transition to happen.</param>
        public void AddAnyTransition(IState to, IPredicate condition)
        {
            anyTransitions.Add(new Transition(GetOrAddNode(to).State, condition));
        }

        /// <summary>
        /// Get or add a node to the state machine.
        /// </summary>
        /// <param name="state">The state to add</param>
        /// <returns>The <see cref="StateNode"/> created for this State.</returns>
        StateNode GetOrAddNode(IState state)
        {
            var node = nodes.GetValueOrDefault(state.id);
            if (node == null)
            {
                node = new StateNode(state);
                nodes.Add(state.id, node);
            }

            return node;
        }
        
        /// <summary>
        /// Node for the state machine.
        /// </summary>
        class StateNode
        {
            /// <summary>
            /// The state this node represents.
            /// </summary>
            public IState State { get; }
            
            /// <summary>
            /// The transitions that can be taken from this state.
            /// </summary>
            public HashSet<ITransition> Transitions { get; }
            
            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="state">State this node represents.</param>
            public StateNode(IState state)
            {
                State = state;
                Transitions = new HashSet<ITransition>();
            }

            /// <summary>
            /// Add a transition to this state.
            /// </summary>
            /// <param name="to">The State to transition to.</param>
            /// <param name="condition">The condition that must be met for this transition to happen.</param>
            public void AddTransition(IState to, IPredicate condition)
            {
                Transitions.Add(new Transition(to, condition));
            }
        }
    }
}