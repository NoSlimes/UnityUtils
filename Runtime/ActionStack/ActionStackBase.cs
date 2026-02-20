using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NoSlimes.UnityUtils.Runtime.ActionStacks.Actions;
using UnityEngine;

namespace NoSlimes.UnityUtils.Runtime.ActionStacks
{
    public interface IActionStack
    {
        IReadOnlyList<IAction> Actions { get; }
        IAction CurrentAction { get; }
    }

    public abstract class ActionStackBase<TActionBase> : MonoBehaviour, IActionStack
        where TActionBase : class, IAction
    {
        private readonly List<TActionBase> stack = new();

        // Tracks which IAction instances have been initialized before
        // I used ConditionalWeakTable to avoid memory leaks, though minor,
        // from accumulating over time from short-lived IAction instances.
        private readonly ConditionalWeakTable<TActionBase, object> initializedActions = new();

        public IReadOnlyList<IAction> Actions => stack;
        public IAction CurrentAction { get; private set; } = null;
        protected TActionBase CurrentActionTyped => CurrentAction as TActionBase;

        public bool IsEmpty => CurrentAction == null && stack.Count == 0;

        public event System.Action<TActionBase> OnActionPushed;
        public event System.Action<TActionBase> OnActionPopped;
        public event System.Action<TActionBase> OnActionBegun;
        public event System.Action<TActionBase> OnActionInterrupted;

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
                    bool firstTime = !initializedActions.TryGetValue(CurrentActionTyped, out _);

                    if (firstTime)
                    {
                        initializedActions.Add(CurrentActionTyped, null);
                        CurrentAction.OnInitialize(); // Replaces IAction.OnBegin(true)
                    }

                    CurrentAction.OnBegin();
                    OnActionBegun?.Invoke(CurrentActionTyped);

                    if (CurrentAction != null)
                    {
                        if (stack.Count > 0 && CurrentAction != stack[0])
                        {
                            var interrupted = CurrentActionTyped;
                            CurrentAction.OnInterrupt();
                            CurrentAction = null;
                            OnActionInterrupted?.Invoke(interrupted);
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
                        if (CurrentAction.IsDone)
                        {
                            var finished = CurrentActionTyped;

                            stack.RemoveAt(0);
                            CurrentAction.OnFinish();
                            CurrentAction = null;

                            OnActionPopped?.Invoke(finished);

                            // REMOVED firstTimeStates.Remove(currentState);
                            // By not removing the state from the initializedActions I have more control over when
                            // a state is reinitialized. 
                            // Reinitialization is controlled by presence in initializedActions,
                            // which can be cleared explicitly when pushing.

                            iterations++;
                            continue; // State finished, restart loop
                        }

                        break; // State still active, exit loop
                    }

                    var interrupted = CurrentActionTyped;
                    CurrentAction.OnInterrupt();
                    CurrentAction = null;
                    OnActionInterrupted?.Invoke(interrupted);

                    iterations++;
                    continue; // Stack changed during update, restart loop
                }

                break; // No changes, exit loop
            }
        }
        #endregion

        /// <summary>
        /// Pushes the specified action onto the stack, optionally reinitializing it before activation.
        /// </summary>
        /// <param name="action">The action to be pushed onto the stack. Cannot be null.</param>
        /// <param name="reinitializeAction">true to reinitialize the action before it is activated; otherwise, false. The default is true.</param>
        public virtual void PushAction(TActionBase action, bool reinitializeAction = true)
        {
            PushInternal(action, reinitializeAction);
        }

        protected void PushInternal(TActionBase action, bool reinitializeAction = true)
        {
            if (action == null) return;

            if (stack.Contains(action))
            {
                // Ignore if it's already on top
                if (stack.Count > 0 && stack[0] == action)
                    return;

                if (reinitializeAction) // If requested, ensure OnStart(true) is called when moved to top
                {
                    initializedActions.Remove(action);
                }

                // If it's buried in the stack, remove it so we can move it to the top.
                stack.Remove(action);
            }

            stack.Insert(0, action);
            OnActionPushed?.Invoke(action);

            //// If the top changed, null the currentState so UpdateStates() 
            //// triggers OnStart() for the new (or moved) state.
            if (CurrentAction != null && CurrentAction != action)
            {
                var interrupted = CurrentActionTyped;
                CurrentAction.OnInterrupt();
                CurrentAction = null;
                OnActionInterrupted?.Invoke(interrupted);
            }
        }

        public void PopAction()
        {
            if (IsEmpty)
                return;

            TActionBase state = stack[0];
            Pop(state);
        }

        public void Pop(TActionBase state)
        {
            if (state == null || IsEmpty)
                return;
            int index = stack.IndexOf(state);
            if (index == -1)
                return; // State not found

            stack.RemoveAt(index);
            state.OnFinish();

            if (CurrentAction == state)
            {
                CurrentAction = null; // Let Update() trigger the next action's OnStart()
            }

            OnActionPopped?.Invoke(state);
        }

        public void ClearStack()
        {
            var popped = stack.ToArray();

            for (int i = 0; i < popped.Length; i++)
            {
                popped[i].OnFinish();
            }

            stack.Clear();
            CurrentAction = null;

            for (int i = 0; i < popped.Length; i++)
            {
                OnActionPopped?.Invoke(popped[i]);
            }
        }
    }
}