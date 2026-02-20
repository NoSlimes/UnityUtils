using NoSlimes.UnityUtils.Runtime.ActionStacks.Actions;
using UnityEngine;

namespace NoSlimes.UnityUtils.Runtime.ActionStacks
{
    public class ActionStack : ActionStackBase<IAction>
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
    }
}