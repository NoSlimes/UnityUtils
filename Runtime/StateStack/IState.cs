using UnityEngine;

namespace NoSlimes.UnityUtils.Runtime.ActionStacks
{
    public partial class StateStack
    {
        public interface IState
        {
            void OnStart(bool firstTime);
            void OnFinish();

            void OnUpdate();

            bool IsDone();
        }

        public abstract class State : IState
        {
            public virtual void OnStart(bool firstTime) { }
            public virtual void OnFinish() { }

            public virtual void OnUpdate() { }

            public abstract bool IsDone();
        }

        public abstract class StateMB : MonoBehaviour, IState
        {
            public virtual void OnStart(bool firstTime) { }
            public virtual void OnFinish() { }

            public virtual void OnUpdate() { }

            public abstract bool IsDone();
        }

        /// <summary>
        /// ScriptableObject-based states can be risky if they hold stateful data.
        /// ScriptableObject.Instantiate could be used to create copies.
        /// </summary>
        public abstract class StateSO : ScriptableObject, IState
        {
            public virtual void OnStart(bool firstTime) { }
            public virtual void OnFinish() { }

            public virtual void OnUpdate() { }

            public abstract bool IsDone();
        }
    }
}