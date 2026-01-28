using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace NoSlimes.UnityUtils.Runtime.ActionStacks
{
    public partial class StateStack : MonoBehaviour
    {
        private static StateStack _main;
        public static StateStack Main
        {
            get
            {
                if (_main == null && Application.isPlaying)
                {
                    GameObject obj = new("StateStack_Main")
                    {
                        hideFlags = HideFlags.DontSave
                    };

                    _main = obj.AddComponent<StateStack>();
                    DontDestroyOnLoad(obj);
                }

                return _main;
            }
        }

        private readonly List<IState> stack = new();
        private readonly HashSet<IState> firstTimeStates = new();
        private IState currentState = null;

        public IReadOnlyList<IState> States => stack;
        public IState CurrentState => currentState;
        public bool IsEmpty => currentState == null && stack.Count == 0;

        #region Unity Lifecycle
        protected virtual void Update()
        {
            UpdateStates();
        }

        private void UpdateStates()
        {
            if (IsEmpty)
                return;

            while (currentState == null && stack.Count > 0)
            {
                currentState = stack[0];

                // Check if this specific instance has EVER been initialized by this stack.
                // In the original, this was based on whether the state was currently 
                // in the stack, which broke the logic for pooled/reused objects (states).
                bool firstTime = !firstTimeStates.Contains(currentState);

                if (firstTime)
                {
                    firstTimeStates.Add(currentState);
                }

                currentState.OnStart(firstTime);

                if (currentState != null)
                {
                    if (stack.Count > 0 && currentState != stack[0])
                    {
                        currentState = null;
                        UpdateStates();
                    }
                }
            }

            if (currentState != null)
            {
                currentState.OnUpdate();

                if (stack.Count > 0 && currentState == stack[0])
                {
                    if (currentState.IsDone())
                    {
                        stack.RemoveAt(0);
                        currentState.OnFinish();

                        // REMOVED firstTimeStates.Remove(currentState);
                        // By NOT removing the state from this HashSet, we ensure that if 
                        // the same object instance is pushed again (e.g. from a pool), 
                        // OnStart(false) is called instead of OnStart(true).

                        currentState = null;
                        UpdateStates(); // Added to allow the stack to cycle to the next state in the same frame
                    }
                }
                else
                {
                    currentState = null;
                }
            }
        }
        #endregion

        public void PushState(IState state)
        {
            if (state == null) return;

            if (stack.Contains(state))
            {
                // If it's already at the top, we don't need to do anything.
                // This prevents re-triggering OnStart every frame.
                if (stack.Count > 0 && stack[0] == state)
                    return;

                // If it's buried in the stack, remove it so we can move it to the top.
                stack.Remove(state);
            }

            // Insert at the top (Index 0)
            stack.Insert(0, state);

            // If the top changed, null the currentState so UpdateStates() 
            // triggers OnStart() for the new (or moved) state.
            if (currentState != state)
            {
                currentState = null;
            }
        }
    }
}