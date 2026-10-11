using NoSlimes.Logging;
using UnityEngine.SceneManagement;

namespace NoSlimes.UnityUtils.Runtime.GameSystems
{
    public interface IGameSystem
    {
        int UpdateOrder { get; }
        bool LogLifecycleEvents { get; }
        bool IsEnabled { get; set; }
        bool IsInitialized { get; }
        void InitializeSystem(GameSystemManager manager, int updateOrder = int.MinValue, bool logLifecycleEvents = false);
        void ShutdownSystem();
        void UpdateSystem();
        void FixedUpdateSystem();
        void LateUpdateSystem();
        void OnPostInitializeSystems();
        void ActiveSceneChanged(Scene newScene);
    }

    public abstract class GameSystem : IGameSystem  
    {
        protected static readonly DLogCategory logGameSystem = new("GameSystem", "#061CCF");
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

                if (LogLifecycleEvents)
                {
                    string state = _isEnabled ? "enabled" : "disabled";
                    DLogger.LogDev($"{GetType().Name} has been {state}.", category: logGameSystem);
                }
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
                if (updateOrder != int.MinValue)
                    UpdateOrder = updateOrder;

                LogLifecycleEvents = logLifecycleEvents;

                IsInitialized = Initialize();
            }

            if (IsInitialized)
            {
                if (logLifecycleEvents)
                    DLogger.LogDev($"{GetType().Name} initialized successfully.", category: logGameSystem);
            }
            else
            {
                DLogger.LogDevError($"Failed to initialize {GetType().Name}. Missing dependencies.");
            }
        }

        public void ShutdownSystem()
        {
            if (!IsInitialized) return;

            Shutdown();
            IsInitialized = false;

            if (LogLifecycleEvents)
                DLogger.LogDev($"{GetType().Name} has been shutdown.", category: logGameSystem);
        }

        public void UpdateSystem()
        {
            if (IsEnabled && IsInitialized)
            {
                Update();
            }
        }

        public void FixedUpdateSystem()
        {
            if (IsEnabled && IsInitialized)
            {
                FixedUpdate();
            }
        }

        public void LateUpdateSystem()
        {
            if (IsEnabled && IsInitialized)
            {
                LateUpdate();
            }
        }

        public virtual void OnPostInitializeSystems() { }
        public virtual void ActiveSceneChanged(Scene newScene) { }

        protected virtual bool Initialize() { return true; }
        protected virtual void Shutdown() => Manager = null;
        protected virtual void OnSystemEnabled() { }
        protected virtual void OnSystemDisabled() { }
        protected virtual void Update() { }
        protected virtual void FixedUpdate() { }
        protected virtual void LateUpdate() { }
    }

}