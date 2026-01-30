using UnityEngine;

namespace NoSlimes.UnityUtils.Runtime.ActionStacking
{
    public partial class ActionStack
    {
        public interface IAction
        {
            void OnInitialize();
            void OnBegin();
            void OnInterrupt();
            void OnFinish();

            void OnUpdate();

            bool IsDone();
        }

        public abstract class Action : IAction
        {
            public virtual void OnInitialize() { }
            public virtual void OnBegin() { }
            public virtual void OnInterrupt() { }
            public virtual void OnFinish() { }

            public virtual void OnUpdate() { }

            public abstract bool IsDone();
        }

        public abstract class ActionMB : MonoBehaviour, IAction
        {
            public virtual void OnInitialize() { }
            public virtual void OnBegin() { }
            public virtual void OnInterrupt() { }
            public virtual void OnFinish() { }

            public virtual void OnUpdate() { }

            public abstract bool IsDone();
        }

        /// <summary>
        /// ScriptableObject-based states can be risky if they hold stateful data.
        /// ScriptableObject.Instantiate could be used to create copies.
        /// </summary>
        public abstract class ActionSO : ScriptableObject, IAction
        {
            public virtual void OnInitialize() { }
            public virtual void OnBegin() { }
            public virtual void OnInterrupt() { }
            public virtual void OnFinish() { }

            public virtual void OnUpdate() { }

            public abstract bool IsDone();
        }
    }
}