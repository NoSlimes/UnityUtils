using System.Collections.Generic;
using UnityEngine;

namespace NoSlimes.UnityUtils.Runtime.ActionStacking
{
    public partial class ActionStack : MonoBehaviour
    {
        private static ActionStack _main;
        public static ActionStack Main
        {
            get
            {
                if (_main == null && Application.isPlaying)
                {
                    GameObject obj = new($"{nameof(ActionStack)}_Main")
                    {
                        hideFlags = HideFlags.DontSave
                    };

                    _main = obj.AddComponent<ActionStack>();
                    DontDestroyOnLoad(obj);
                }

                return _main;
            }
        }

        private readonly List<IAction> stack = new();
        private readonly HashSet<IAction> firstTimeActions = new();

        public IReadOnlyList<IAction> Actions => stack;
        public IAction CurrentAction { get; private set; } = null;
        public bool IsEmpty => CurrentAction == null && stack.Count == 0;

        #region Unity Lifecycle
        protected virtual void Update()
        {
            UpdateActions();
        }

        private void UpdateActions()
        {
            if (IsEmpty)
                return;

            while (CurrentAction == null && stack.Count > 0)
            {
                CurrentAction = stack[0];

                // Check if this specific instance has EVER been initialized by this stack.
                // In the original, this was based on whether the state was currently 
                // in the stack, which broke the logic for pooled/reused objects (states).
                bool firstTime = !firstTimeActions.Contains(CurrentAction);

                if (firstTime)
                {
                    firstTimeActions.Add(CurrentAction);
                }

                CurrentAction.OnStart(firstTime);

                if (CurrentAction != null)
                {
                    if (stack.Count > 0 && CurrentAction != stack[0])
                    {
                        CurrentAction = null;
                        UpdateActions();
                    }
                }
            }

            if (CurrentAction != null)
            {
                CurrentAction.OnUpdate();

                if (stack.Count > 0 && CurrentAction == stack[0])
                {
                    if (CurrentAction.IsDone())
                    {
                        stack.RemoveAt(0);
                        CurrentAction.OnFinish();

                        // REMOVED firstTimeStates.Remove(currentState);
                        // By NOT removing the state from this HashSet, we ensure that if 
                        // the same object instance is pushed again (e.g. from a pool), 
                        // OnStart(false) is called instead of OnStart(true).

                        CurrentAction = null;
                        UpdateActions(); // Added to allow the stack to cycle to the next state in the same frame
                    }
                }
                else
                {
                    CurrentAction = null;
                }
            }
        }
        #endregion

        public void PushAction(IAction state, bool reinitializeState = false)
        {
            if (state == null) return;

            if (stack.Contains(state))
            {
                // Ignore if it's already on top
                if (stack.Count > 0 && stack[0] == state)
                    return;

                if (reinitializeState) // If requested, ensure OnStart(true) is called when moved to top
                {
                    firstTimeActions.Remove(state); 
                }

                // If it's buried in the stack, remove it so we can move it to the top.
                stack.Remove(state);
            }

            stack.Insert(0, state);

            // If the top changed, null the currentState so UpdateStates() 
            // triggers OnStart() for the new (or moved) state.
            if (CurrentAction != state)
            {
                CurrentAction = null;
            }
        }
    }
}