using UnityEngine;

namespace NoSlimes.UnityUtils.Runtime.ActionStacks.Actions
{
    public interface IAction
    {
        void OnInitialize();
        void OnBegin();
        void OnInterrupt();
        void OnFinish();

        void OnUpdate();
        void OnLateUpdate();

        public bool IsDone { get; }
    }

    public abstract class Action : IAction
    {
        public virtual bool IsDone { get; protected set; }

        public virtual void OnInitialize() { }
        public virtual void OnBegin() { }
        public virtual void OnInterrupt() { }
        public virtual void OnFinish() { }

        public virtual void OnUpdate() { }
        public virtual void OnLateUpdate() { }
    }

    public abstract class ActionMB : MonoBehaviour, IAction
    {
        public virtual bool IsDone { get; protected set; }

        public virtual void OnInitialize() { }
        public virtual void OnBegin() { }
        public virtual void OnInterrupt() { }
        public virtual void OnFinish() { }

        public virtual void OnUpdate() { }
        public virtual void OnLateUpdate() { }
    }

    /// <summary>
    /// ScriptableObject-based states can be risky if they hold stateful data.
    /// ScriptableObject.Instantiate could be used to create copies.
    /// </summary>
    public abstract class ActionSO : ScriptableObject, IAction
    {
        public virtual bool IsDone { get; protected set; }

        public virtual void OnInitialize() { }
        public virtual void OnBegin() { }
        public virtual void OnInterrupt() { }
        public virtual void OnFinish() { }

        public virtual void OnUpdate() { }
        public virtual void OnLateUpdate() { }
    }
}