using NoSlimes.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NoSlimes.UnityUtils.Runtime.GameSystems
{
    public class GameSystemManager : MonoBehaviour
    {
        private static readonly DLogCategory logGameSystemManager = new("GameSystemManager", Color.white);

        [SerializeField] private bool logLifecycleEvents = false;

        [Header("System Configs")]
        [SerializeField] private List<GameConfig> systemConfigs = new();

        private static GameSystemManager _instance;
        public static GameSystemManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<GameSystemManager>();
                    if (_instance == null)
                    {
                        DLogger.LogDevError("No GameSystemManager instance found in the scene.", category: logGameSystemManager);
                    }
                }

                return _instance;
            }
            private set => _instance = value;
        }

        private readonly Dictionary<Type, IGameSystem> gameplaySystems = new();
        private List<IGameSystem> sortedSystems = new();

        private readonly Dictionary<int, HashSet<IGameSystem>> systemsFor = new();

        //System type cache
        private readonly HashSet<Type> autoSystemTypeCache = new();

        private bool isInitialized = false;
        private bool suspendSorting = false;

        public void RegisterGameSystem(IGameSystem system, int updateOrder = int.MinValue)
        {
            if (system == null) return;

            var type = system.GetType();
            if (gameplaySystems.ContainsKey(type))
            {
                DLogger.LogDevWarning($"A system of type:{type.Name} is already registered. Skipping duplicate.", category: logGameSystemManager);
                return;
            }

            system.InitializeSystem(this, updateOrder, logLifecycleEvents);
            gameplaySystems.Add(type, system);

            if (system.GetType().GetCustomAttribute<AutoGameSystemAttribute>() is AutoGameSystemAttribute autoAttr)
            {
                foreach (var buildIndex in autoAttr.ActiveSceneBuildIndexes)
                {
                    systemsFor.TryAdd(buildIndex, new HashSet<IGameSystem>());
                    systemsFor[buildIndex].Add(system);
                }
            }

            if (isInitialized)
            {
                DLogger.LogDevWarning($"System '{system.GetType().Name}' registered after initialization. Calling OnPostInitializeSystems manually.", category: logGameSystemManager);
                system.OnPostInitializeSystems();
            }

            if (!suspendSorting)
                SortSystems();
        }

        public void UnregisterGameSystem(IGameSystem system)
        {
            if (system == null)
                return;

            gameplaySystems.Remove(system.GetType());
            system.ShutdownSystem();

            SortSystems();
        }

        public T GetGameSystem<T>() where T : class, IGameSystem
        {
            return gameplaySystems.TryGetValue(typeof(T), out IGameSystem system) && system is T typed ? typed : null;
        }

        public static T FindGameSystem<T>() where T : class, IGameSystem
        {
            if(Instance == null)
            {
                DLogger.LogDevError("GameSystemManager instance is not initialized. Cannot retrieve gameplay systems.", category: logGameSystemManager);
                return null;
            }

            return Instance.GetGameSystem<T>();
        }

        /// <summary>
        /// First assigned config of type T, or null. Shared freely: several
        /// systems can pull the same config, one system can pull several.
        /// </summary>
        public T GetConfig<T>() where T : GameConfig
        {
            T found = null;

            foreach (GameConfig config in systemConfigs)
            {
                if (config is T typed)
                {
                    if (found != null)
                    {
                        DLogger.LogDevWarning($"Multiple {typeof(T).Name} configs assigned; using the first.", category: logGameSystemManager);
                        break;
                    }

                    found = typed;
                }
            }

            return found;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                DLogger.LogDevWarning("Multiple GameSystemManager instances detected. Destroying duplicate.", category: logGameSystemManager);
                Destroy(this.gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(this.gameObject);

            gameplaySystems.Clear();

            var autoSystems = GetAutoInitializedSystems();
            var sortedAutoSystems = autoSystems.OrderBy(s => s.updateOrder).ToList();

            foreach (var (systemInstance, updateOrder) in sortedAutoSystems)
            {
                RegisterGameSystem(systemInstance, updateOrder);
            }

            RegisterSceneSystems();
        }

        private void Start()
        {
            isInitialized = true;
            OnAllSystemsInitialized();
        }

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            int oldIndex = oldScene.buildIndex;
            int newIndex = newScene.buildIndex;

            var systemsToUnregister = systemsFor.TryGetValue(oldIndex, out var oldSystems) ? oldSystems : null;
            var systemsToRegister = systemsFor.TryGetValue(newIndex, out var newSystems) ? newSystems : null;

            if (logLifecycleEvents)
                DLogger.LogDev($"Scene changed from '{oldScene.name}' (Index: {oldIndex}) to '{newScene.name}' (Index: {newIndex}).\n" +
                $"Unregistering Systems: {BuildSeparatedTypeNamesString(systemsToUnregister?.ToList() ?? new List<IGameSystem>())}\n" +
                    $"Registering Systems: {BuildSeparatedTypeNamesString(systemsToRegister?.ToList() ?? new List<IGameSystem>())}", category: logGameSystemManager);

            suspendSorting = true;

            if (systemsToUnregister != null)
                foreach (var system in systemsToUnregister)
                    UnregisterGameSystem(system);

            if (systemsToRegister != null)
            {
                foreach (var system in systemsToRegister)
                {
                    // Re-encountering a live system on scene change is expected
                    // (startup registers the first scene in Awake). Skip quietly.
                    if (!gameplaySystems.ContainsKey(system.GetType()))
                        RegisterGameSystem(system);
                }
            }

            suspendSorting = false;
            SortSystems();

            PruneDeadMonoSystems();
            RegisterSceneSystems();

            foreach (var system in gameplaySystems.Values)
                system.ActiveSceneChanged(newScene);
        }

        private string BuildSeparatedTypeNamesString(List<IGameSystem> systems, string separator = ", ")
        {
            return string.Join(separator, systems.Select(s => s.GetType().Name));
        }

        private HashSet<(IGameSystem systemInstance, int updateOrder)> GetAutoInitializedSystems()
        {
            var autoInitializedSystems = new HashSet<(IGameSystem, int)>();

            if (autoSystemTypeCache.Count == 0)
            {
                // Runtime scan: no UnityEngine API enumerates loaded game assemblies,
                // so AppDomain it is. Per-assembly try/catch: an unloadable assembly
                // must not abort discovery of the rest.
#pragma warning disable UAC0005
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(SafeGetTypes)
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(IGameSystem).IsAssignableFrom(t))
                    .Where(t => !typeof(MonoBehaviour).IsAssignableFrom(t))
                    .Where(t => t.GetCustomAttribute<AutoGameSystemAttribute>() != null);
#pragma warning restore UAC0005

                foreach (var type in types)
                {
                    autoSystemTypeCache.Add(type);
                }
            }

            var currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            foreach (var type in autoSystemTypeCache)
            {
                var attribute = type.GetCustomAttribute<AutoGameSystemAttribute>();

                if (attribute.ActiveSceneBuildIndexes.Count > 0)
                {
                    if (!attribute.ActiveSceneBuildIndexes.Contains(currentSceneIndex))
                    {
                        continue;
                    }
                }

                if (Activator.CreateInstance(type) is IGameSystem system)
                {
                    autoInitializedSystems.Add((system, attribute.UpdateOrder));
                }
            }

            return autoInitializedSystems;
        }

        private void OnAllSystemsInitialized()
        {
            foreach (var system in gameplaySystems.Values)
            {
                system.OnPostInitializeSystems();
            }
        }

        private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null);
            }
        }

        private void SortSystems()
        {
            sortedSystems = gameplaySystems.Values.OrderBy(s => s.UpdateOrder).ToList();
        }

        private void RegisterSceneSystems()
        {
            foreach (MonoGameSystem system in FindObjectsByType<MonoGameSystem>(FindObjectsInactive.Include))
            {
                if (system == null) continue;

                if (!gameplaySystems.ContainsKey(system.GetType()))
                    RegisterGameSystem(system);
            }
        }

        private void PruneDeadMonoSystems()
        {
            List<Type> dead = null;

            foreach (var kvp in gameplaySystems)
            {
                if (kvp.Value is MonoBehaviour behaviour && behaviour == null)
                    (dead ??= new List<Type>()).Add(kvp.Key);
            }

            if (dead == null) return;

            foreach (Type type in dead)
            {
                gameplaySystems.Remove(type);

                foreach (var set in systemsFor.Values)
                    set.RemoveWhere(s => s is MonoBehaviour m && m == null);
            }

            SortSystems();
        }

        private void Update()
        {
            foreach (var system in sortedSystems)
            {
                system.UpdateSystem();
            }
        }

        private void FixedUpdate()
        {
            foreach (var system in sortedSystems)
            {
                system.FixedUpdateSystem();
            }
        }

        private void LateUpdate()
        {
            foreach (var system in sortedSystems)
            {
                system.LateUpdateSystem();
            }
        }
    }
}