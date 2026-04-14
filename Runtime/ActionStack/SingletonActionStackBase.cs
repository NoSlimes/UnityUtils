using NoSlimes.UnityUtils.Runtime.ActionStacks.Actions;
using System.Collections.Generic;
using UnityEngine;

namespace NoSlimes.UnityUtils.Runtime.ActionStacks
{
    public abstract class SingletonActionStackBase<TActionStack, TActionBase> : MonoBehaviour, IActionStack<TActionBase>
        where TActionStack : MonoBehaviour, IActionStack
        where TActionBase : class, IAction
    {
        private readonly ActionStack<TActionBase> actionStack = new();

        protected virtual bool PersistAcrossScenes => false;

        private static TActionStack _main;
        public static TActionStack Main
        {
            get
            {
                if (_main == null && Application.isPlaying)
                {
                    GameObject obj = new($"{typeof(TActionStack).Name}_Main");
                    _main = obj.AddComponent<TActionStack>();

                    if ((_main as SingletonActionStackBase<TActionStack, TActionBase>).PersistAcrossScenes)
                    {
                        DontDestroyOnLoad(obj);
                    }
                }
                return _main;
            }
        }

        private void Update() => actionStack.Update();

        private void LateUpdate() => actionStack.LateUpdate();

        private void OnDestroy()
        {
            if (_main == this)
                _main = null;

            actionStack.ClearStack();
        }

        public IReadOnlyList<IAction> Actions => actionStack.Actions;

        public IAction CurrentAction => actionStack.CurrentAction;

        public void PushAction(TActionBase action, bool reinitializeAction = true) => actionStack.PushAction(action, reinitializeAction);
        public void Pop() => actionStack.Pop();
        public void Pop(TActionBase action) => actionStack.Pop(action);
    }
}