using NoSlimes.UnityUtils.Runtime.ActionStacks.Actions;
using UnityEngine;

namespace NoSlimes.UnityUtils.Runtime.ActionStacks
{
    public abstract class SingletonActionStack<TActionStack, TActionBase> : ActionStackBase<TActionBase>
        where TActionStack : MonoBehaviour, IActionStack
        where TActionBase : class, IAction
    {
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

                    if ((_main as SingletonActionStack<TActionStack, TActionBase>).PersistAcrossScenes)
                    {
                        DontDestroyOnLoad(obj);
                    }
                }
                return _main;
            }
        }
    }
}