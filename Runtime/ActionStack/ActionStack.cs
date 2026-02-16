using System.Collections.Generic;
using UnityEngine;
using System.Runtime.CompilerServices;
using UnityEditor.Experimental.GraphView;

namespace NoSlimes.UnityUtils.Runtime.ActionStacks
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

        // Tracks which IAction instances have been initialized before
        // I used ConditionalWeakTable to avoid memory leaks, though minor,
        // from accumulating over time from short-lived IAction instances.
        private readonly ConditionalWeakTable<IAction, object> initializedActions = new();

        public IReadOnlyList<IAction> Actions => stack;
        public IAction CurrentAction { get; private set; } = null;
        public bool IsEmpty => CurrentAction == null && stack.Count == 0;

        public event System.Action<IAction> OnActionPushed;
        public event System.Action<IAction> OnActionPopped;
        public event System.Action<IAction> OnActionBegun;
        public event System.Action<IAction> OnActionInterrupted;

        #region Unity Lifecycle
        protected virtual void Update()
        {
            UpdateActions(false);
        }

        protected virtual void LateUpdate()
        {
            UpdateActions(true);
        }

        private void UpdateActions(bool useLateUpdate)
        {
            if (IsEmpty)
                return;

            int iterations = 0;
            const int MAX_ITERATIONS = 100;

            while (iterations < MAX_ITERATIONS)
            {

                if (CurrentAction == null)
                {
                    if (stack.Count == 0)
                        break; // No actions to process

                    CurrentAction = stack[0];

                    // Check if this specific instance has EVER been initialized by this stack.
                    // I wanted to support reusing state instances without having them reinitialize every time.
                    bool firstTime = !initializedActions.TryGetValue(CurrentAction, out _);

                    if (firstTime)
                    {
                        initializedActions.Add(CurrentAction, null);
                        CurrentAction.OnInitialize(); // Replaces IAction.OnBegin(true)
                    }

                    CurrentAction.OnBegin();
                    OnActionBegun?.Invoke(CurrentAction);

                    if (CurrentAction != null)
                    {
                        if (stack.Count > 0 && CurrentAction != stack[0])
                        {
                            CurrentAction?.OnInterrupt();
                            CurrentAction = null;
                            iterations++;
                            continue; // Stack changed during OnBegin, restart loop
                        }
                    }
                }

                if (CurrentAction != null)
                {
                    if (useLateUpdate)
                        CurrentAction.OnLateUpdate();
                    else
                        CurrentAction.OnUpdate();

                    if (stack.Count > 0 && CurrentAction == stack[0])
                    {
                        if (CurrentAction.IsDone())
                        {
                            stack.RemoveAt(0);
                            CurrentAction.OnFinish();
                            OnActionPopped?.Invoke(CurrentAction);

                            // REMOVED firstTimeStates.Remove(currentState);
                            // By not removing the state from the initializedActions I have more control over when
                            // a state is reinitialized. 
                            // Reinitialization is controlled by presence in initializedActions,
                            // which can be cleared explicitly when pushing.

                            CurrentAction = null;
                            iterations++;
                            continue; // State finished, restart loop
                        }

                        break; // State still active, exit loop
                    }

                    CurrentAction.OnInterrupt();
                    OnActionInterrupted?.Invoke(CurrentAction);

                    CurrentAction = null;
                    iterations++;
                    continue; // Stack changed during update, restart loop
                }

                break; // No changes, exit loop
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        /// <param name="reinitializeAction"></param>
        public void PushAction(IAction state, bool reinitializeAction = true)
        {
            if (state == null) return;

            if (stack.Contains(state))
            {
                // Ignore if it's already on top
                if (stack.Count > 0 && stack[0] == state)
                    return;

                if (reinitializeAction) // If requested, ensure OnStart(true) is called when moved to top
                {
                    initializedActions.Remove(state);
                }

                // If it's buried in the stack, remove it so we can move it to the top.
                stack.Remove(state);
            }

            stack.Insert(0, state);
            OnActionPushed?.Invoke(state);

            //// If the top changed, null the currentState so UpdateStates() 
            //// triggers OnStart() for the new (or moved) state.
            if (CurrentAction != null && CurrentAction != state)
            {
                CurrentAction.OnInterrupt();
                OnActionInterrupted?.Invoke(CurrentAction);
            }
            CurrentAction = null;
        }
    }
}