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

    public interface IActionStack<T> : IActionStack
    {
        void PushAction(T action, bool reinitializeAction = true);
        void Pop();
        void Pop(T action);
    }

    public class ActionStack<TActionBase> : IActionStack
        where TActionBase : class, IAction
    {
        private readonly List<TActionBase> stack = new();
        private readonly ConditionalWeakTable<TActionBase, object> initializedActions = new();
        private readonly HashSet<TActionBase> interrupted = new();

        public IReadOnlyList<IAction> Actions => stack;

        public IAction CurrentAction => CurrentActionTyped;
        public TActionBase CurrentActionTyped { get; private set; }

        public bool IsEmpty => CurrentActionTyped == null && stack.Count == 0;

        public event System.Action<TActionBase> OnActionPushed;
        public event System.Action<TActionBase> OnActionPopped;
        public event System.Action<TActionBase> OnActionBegun;
        public event System.Action<TActionBase> OnActionInterrupted;
        public event System.Action<TActionBase> OnActionResumed;

        public void Update() => UpdateActions(false);
        public void LateUpdate() => UpdateActions(true);

        private void UpdateActions(bool useLateUpdate)
        {
            if (IsEmpty)
                return;

            int iterations = 0;
            const int MAX_ITERATIONS = 100;

            while (iterations < MAX_ITERATIONS)
            {
                // Current action
                if (CurrentActionTyped == null)
                {
                    if (stack.Count == 0)
                        break;

                    CurrentActionTyped = stack[0];

                    bool firstTime = !initializedActions.TryGetValue(CurrentActionTyped, out _);
                    bool wasInterrupted = interrupted.Contains(CurrentActionTyped);

                    if (firstTime)
                    {
                        initializedActions.Add(CurrentActionTyped, null);
                        CurrentActionTyped.OnInitialize();
                    }

                    if (wasInterrupted)
                    {
                        interrupted.Remove(CurrentActionTyped);
                        CurrentActionTyped.OnResume();
                        OnActionResumed?.Invoke(CurrentActionTyped);
                    }
                    else
                    {
                        CurrentActionTyped.OnBegin();
                        OnActionBegun?.Invoke(CurrentActionTyped);
                    }

                    // Stack changed during enter
                    if (stack.Count > 0 && CurrentActionTyped != stack[0])
                    {
                        var interruptedAction = CurrentActionTyped;

                        CurrentActionTyped.OnInterrupt();
                        OnActionInterrupted?.Invoke(interruptedAction);

                        interrupted.Add(interruptedAction);

                        CurrentActionTyped = null;
                        iterations++;
                        continue;
                    }
                }

                // Update current action
                if (CurrentActionTyped != null)
                {
                    if (useLateUpdate)
                        CurrentActionTyped.OnLateUpdate();
                    else
                        CurrentActionTyped.OnUpdate();

                    // Stack integrity check
                    if (stack.Count == 0 || CurrentActionTyped != stack[0])
                    {
                        var interruptedAction = CurrentActionTyped;

                        CurrentActionTyped.OnInterrupt();
                        OnActionInterrupted?.Invoke(interruptedAction);

                        interrupted.Add(interruptedAction);

                        CurrentActionTyped = null;
                        iterations++;
                        continue;
                    }

                    // Completion
                    if (CurrentActionTyped.IsDone)
                    {
                        var finished = CurrentActionTyped;

                        stack.RemoveAt(0);
                        CurrentActionTyped.OnFinish();

                        interrupted.Remove(CurrentActionTyped);

                        CurrentActionTyped = null;

                        OnActionPopped?.Invoke(finished);

                        iterations++;
                        continue;
                    }

                    break;
                }

                break;
            }
        }

        public void PushAction(TActionBase action, bool reinitializeAction = true)
        {
            if (action == null)
                return;

            if (stack.Contains(action))
            {
                if (stack.Count > 0 && ReferenceEquals(stack[0], action))
                    return;

                if (reinitializeAction)
                    initializedActions.Remove(action);

                stack.Remove(action);
            }

            stack.Insert(0, action);
            OnActionPushed?.Invoke(action);

            if (CurrentActionTyped != null && CurrentActionTyped != action)
            {
                var interruptedAction = CurrentActionTyped;

                CurrentActionTyped.OnInterrupt();
                OnActionInterrupted?.Invoke(interruptedAction);

                interrupted.Add(interruptedAction);

                CurrentActionTyped = null;
            }
        }

        public void Pop()
        {
            if (IsEmpty)
                return;

            Pop(CurrentActionTyped);
        }

        public void Pop(TActionBase action)
        {
            if (action == null || stack.Count == 0)
                return;

            int index = stack.IndexOf(action);
            if (index == -1)
                return;

            bool isCurrent = ReferenceEquals(CurrentActionTyped, action);

            if (isCurrent)
            {
                action.OnInterrupt();
                OnActionInterrupted?.Invoke(action);

                interrupted.Add(action);

                CurrentActionTyped = null;
            }

            stack.RemoveAt(index);
            action.OnFinish();

            interrupted.Remove(action);

            OnActionPopped?.Invoke(action);
        }

        public void ClearStack()
        {
            var copy = stack.ToArray();

            foreach (var a in copy)
            {
                if (ReferenceEquals(CurrentActionTyped, a))
                {
                    a.OnInterrupt();
                    OnActionInterrupted?.Invoke(a);

                    interrupted.Add(a);

                    CurrentActionTyped = null;
                }

                a.OnFinish();
                interrupted.Remove(a);

                OnActionPopped?.Invoke(a);
            }

            stack.Clear();
            initializedActions.Clear();
            interrupted.Clear();
        }
    }
}