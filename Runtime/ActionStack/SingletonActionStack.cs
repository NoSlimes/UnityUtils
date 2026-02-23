using NoSlimes.UnityUtils.Runtime.ActionStacks.Actions;
using UnityEngine;

namespace NoSlimes.UnityUtils.Runtime.ActionStacks
{
    public abstract class SingletonActionStack<TActionStack, TActionBase> : ActionStackBase<TActionBase>
        where TActionStack : MonoBehaviour, IActionStack
        where TActionBase : class, IAction
    {
        private static TActionStack _main;

        protected virtual bool PersistAcrossScenes => false;

        public static TActionStack Main
        {
            get
            {
                if (_main == null && Application.isPlaying)
                {
                    GameObject obj = new($"{typeof(TActionStack).Name}_Main")
                    {
                        hideFlags = HideFlags.DontSave
                    };
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