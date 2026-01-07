using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace NoSlimes.UnityUtils.Gameplay.ActionStack
{
    public class ActionStack : MonoBehaviour
    {
        private static ActionStack _main;
        public static ActionStack Main
        {
            get
            {
                if (_main == null)
                {
                    GameObject obj = new("ActionStackManager_Main");
                    _main = obj.AddComponent<ActionStack>();
                    DontDestroyOnLoad(obj);
                }

                return _main;
            }
        }

        private readonly List<IAction> actionStack = new();
        private readonly HashSet<IAction> firstTimeActions = new();
        private IAction currentAction = null;


        public ReadOnlyCollection<IAction> Actions => new(actionStack);
        public bool IsEmpty => currentAction == null && actionStack.Count == 0;

        #region Unity Lifecycle
        private void Update()
        {
            UpdateActions();
        }

        private void UpdateActions()
        {
            if (IsEmpty)
                return;

            while (currentAction == null && actionStack.Count > 0)
            {
                currentAction = actionStack[0];
                bool firstTime = !firstTimeActions.Contains(currentAction);
                firstTimeActions.Add(currentAction);
                currentAction.OnStart(firstTime);

                if (currentAction != null)
                {
                    if (actionStack.Count > 0 && currentAction != actionStack[0])
                    {
                        currentAction = null;
                        UpdateActions();
                    }
                }
            }

            if (currentAction != null)
            {
                currentAction.OnUpdate();

                if (actionStack.Count > 0 && currentAction == actionStack[0])
                {
                    if (currentAction.IsDone())
                    {
                        actionStack.RemoveAt(0);
                        currentAction.OnFinish();
                        firstTimeActions.Remove(currentAction);
                        currentAction = null;
                    }
                }
                else
                {
                    currentAction = null;
                }
            }
        }
        #endregion

        public void PushAction(IAction action)
        {
            if (action != null)
            {
                if (actionStack.Contains(action))
                {
                    Debug.LogWarning("ActionStackManager: Attempted to push an action that is already in the stack.");
                    return;
                }

                actionStack.Insert(0, action);

                if (currentAction != null && currentAction != action)
                {
                    currentAction = null;
                }
            }
        }
    }
}