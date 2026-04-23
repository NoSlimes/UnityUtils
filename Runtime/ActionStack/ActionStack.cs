using NoSlimes.UnityUtils.Runtime.ActionStacks.Actions;
using System.Collections.Generic;
using UnityEngine;

namespace NoSlimes.UnityUtils.Runtime.ActionStacks
{

    public class ActionStack : MonoBehaviour, IActionStack<IAction>
    {
        private readonly ActionStack<IAction> actionStack = new();

        public IReadOnlyList<IAction> Actions => actionStack.Actions;
        public IAction CurrentAction => actionStack.CurrentAction;

        private void Update() => actionStack.Update();
        private void LateUpdate() => actionStack.LateUpdate();
        private void OnDestroy()
        {
            actionStack.Clear();
        }

        public void PushAction(IAction action, bool reinitializeAction = true) => actionStack.PushAction(action, reinitializeAction);
        public void Pop(IAction action) => actionStack.Pop(action);
        public void Pop() => actionStack.Pop();
        public void Clear() => actionStack.Clear();
    }
}