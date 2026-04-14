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

    public class ActionStack<TActionBase> : IActionStack<TActionBase>
     where TActionBase : class, IAction
    {
        private readonly List<TActionBase> stack = new();

        private readonly ConditionalWeakTable<TActionBase, object> initializedActions = new();

        public IReadOnlyList<IAction> Actions => stack;

        public IAction CurrentAction => stack.Count > 0 ? stack[0] : null;
        public TActionBase CurrentActionTyped => stack.Count > 0 ? stack[0] : null;

        public bool IsEmpty => stack.Count == 0;

        public event System.Action<TActionBase> OnActionPushed;
        public event System.Action<TActionBase> OnActionPopped;
        public event System.Action<TActionBase> OnActionBegun;
        public event System.Action<TActionBase> OnActionInterrupted;

        public virtual void Update() => UpdateActions(false);
        public virtual void LateUpdate() => UpdateActions(true);
        
        private void UpdateActions(bool useLateUpdate)
        {
            if (stack.Count == 0)
                return;

            int iterations = 0;
            const int MAX_ITERATIONS = 100;

            while (iterations < MAX_ITERATIONS)
            {
                if (stack.Count == 0)
                    break;

                var current = stack[0];

                if (!ReferenceEquals(CurrentAction, current))
                    break;

                bool firstTime = !initializedActions.TryGetValue(current, out _);

                if (firstTime)
                {
                    initializedActions.Add(current, null);
                    current.OnInitialize();
                    current.OnBegin();
                    OnActionBegun?.Invoke(current);
                }

                if (useLateUpdate)
                    current.OnLateUpdate();
                else
                    current.OnUpdate();

                if (stack.Count == 0 || !ReferenceEquals(stack[0], current))
                {
                    HandleInterrupt(current);
                    iterations++;
                    continue;
                }

                if (current.IsDone)
                {
                    HandleFinish(current);
                    iterations++;
                    continue;
                }

                break;
            }

            if (iterations >= MAX_ITERATIONS)
            {
                Debug.LogError("[ActionStackBase] Infinite loop detected in UpdateActions.");
            }
        }

        private void HandleInterrupt(TActionBase action)
        {
            action.OnInterrupt();
            OnActionInterrupted?.Invoke(action);
        }

        private void HandleFinish(TActionBase action)
        {
            stack.RemoveAt(0);
            action.OnFinish();
            OnActionPopped?.Invoke(action);
        }

        public virtual void PushAction(TActionBase action, bool reinitializeAction = true)
        {
            if (action == null)
                return;

            if (stack.Contains(action))
            {
                if (stack.Count > 0 && ReferenceEquals(stack[0], action))
                    return;

                stack.Remove(action);

                if (reinitializeAction)
                    initializedActions.Remove(action);
            }

            bool hadCurrent = stack.Count > 0;

            if (hadCurrent)
            {
                var previous = stack[0];
                HandleInterrupt(previous);
            }

            stack.Insert(0, action);

            OnActionPushed?.Invoke(action);
        }

        public void Pop()
        {
            if (stack.Count == 0)
                return;

            Pop(stack[0]);
        }

        public void Pop(TActionBase action)
        {
            if (action == null || stack.Count == 0)
                return;

            int index = stack.IndexOf(action);
            if (index == -1)
                return;

            bool isCurrent = ReferenceEquals(stack[0], action);

            if (isCurrent)
                HandleInterrupt(action);

            stack.RemoveAt(index);

            action.OnFinish();
            OnActionPopped?.Invoke(action);
        }

        public void ClearStack()
        {
            ForceClear(true);
        }

        private void ForceClear(bool fireEvents)
        {
            var copy = stack.ToArray();

            foreach (var a in copy)
            {
                a.OnInterrupt();
                a.OnFinish();

                if (fireEvents)
                    OnActionPopped?.Invoke(a);
            }

            stack.Clear();
            initializedActions.Clear();
        }
    }
}