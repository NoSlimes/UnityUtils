using UnityEngine;

namespace NoSlimes.UnityUtils.Gameplay.ActionStack
{
    public interface IAction
    {
        void OnUpdate();

        void OnStart(bool firstTime);
        void OnFinish();

        bool IsDone();
    }

    public abstract class Action : IAction
    {
        public virtual void OnUpdate() { }

        public virtual void OnStart(bool firstTime) { }
        public virtual void OnFinish() { }
        public abstract bool IsDone();
    }

    public abstract class ActionMonoBehaviour : MonoBehaviour, IAction
    {
        public virtual void OnUpdate() { }

        public virtual void OnStart(bool firstTime) { }
        public virtual void OnFinish() { }
        public abstract bool IsDone();
    }

    public abstract class ActionScriptableObject : ScriptableObject, IAction
    {
        public virtual void OnUpdate() { }

        public virtual void OnStart(bool firstTime) { }
        public virtual void OnFinish() { }
        public abstract bool IsDone();
    }
}