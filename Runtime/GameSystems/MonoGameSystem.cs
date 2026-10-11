using UnityEngine;
using UnityEngine.SceneManagement;

namespace NoSlimes.UnityUtils.Runtime.GameSystems
{
    /// <summary>
    /// MonoBehaviour gameplay system: inspector data, coroutines and scene refs,
    /// with the framework pumps driven by the manager.
    /// Tick hooks are named SystemUpdate/SystemFixedUpdate/SystemLateUpdate on
    /// purpose: Unity message-dispatches anything named Update, which would
    /// double-drive the logic. Never define MonoBehaviour Update here.
    /// Auto-registered from the scene; destroyed instances are pruned on scene
    /// change. Don't destroy one at runtime — disable it via IsEnabled instead.
    /// </summary>
    public abstract class MonoGameSystem : MonoBehaviour, IGameSystem
    {
        [SerializeField] private int inspectorUpdateOrder = 0;
        [SerializeField] private bool logLifecycleEventsInspector = false;

        private bool _isEnabled = true;

        public int UpdateOrder { get; protected set; } = 0;
        public bool LogLifecycleEvents { get; protected set; } = false;

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                if (_isEnabled)
                    OnSystemEnabled();
                else
                    OnSystemDisabled();
            }
        }
        public bool IsInitialized { get; private set; } = false;

        protected GameSystemManager Manager { get; private set; }

        public void InitializeSystem(GameSystemManager manager, int updateOrder = int.MinValue, bool logLifecycleEvents = false)
        {
            Manager = manager;
            if (Manager == null)
            {
                IsInitialized = false;
            }
            else
            {
                UpdateOrder = updateOrder != int.MinValue ? updateOrder : inspectorUpdateOrder;
                LogLifecycleEvents = logLifecycleEvents || logLifecycleEventsInspector;

                IsInitialized = Initialize();
            }
        }

        public void ShutdownSystem()
        {
            if (!IsInitialized) return;

            Shutdown();
            IsInitialized = false;
        }

        public void UpdateSystem()
        {
            if (IsEnabled && IsInitialized)
            {
                SystemUpdate();
            }
        }

        public void FixedUpdateSystem()
        {
            if (IsEnabled && IsInitialized)
            {
                SystemFixedUpdate();
            }
        }

        public void LateUpdateSystem()
        {
            if (IsEnabled && IsInitialized)
            {
                SystemLateUpdate();
            }
        }

        public virtual void OnPostInitializeSystems() { }
        public virtual void ActiveSceneChanged(Scene newScene) { }

        protected virtual bool Initialize() { return true; }
        protected virtual void Shutdown() => Manager = null;
        protected virtual void OnSystemEnabled() { }
        protected virtual void OnSystemDisabled() { }
        protected virtual void SystemUpdate() { }
        protected virtual void SystemFixedUpdate() { }
        protected virtual void SystemLateUpdate() { }
    }

}
