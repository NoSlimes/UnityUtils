using NoSlimes.Logging;
using System;
using System.Collections.Generic;

namespace NoSlimes.UnityUtils.Runtime.GameSystems
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AutoGameSystemAttribute : Attribute
    {
        public int UpdateOrder { get; }
        public HashSet<int> ActiveSceneBuildIndexes { get; } = new(); // Empty set means active in all scenes

        public AutoGameSystemAttribute(int updateOrder = 0)
        {
            UpdateOrder = updateOrder;
        }

        public AutoGameSystemAttribute(int updateOrder = 0, params int[] activeSceneBuildIndexes)
        {
            UpdateOrder = updateOrder;

            foreach (var index in activeSceneBuildIndexes)
            {
                ActiveSceneBuildIndexes.Add(index);
            }
        }

        public AutoGameSystemAttribute(int updateOrder = 0, params string[] activeSceneNames)
        {
            UpdateOrder = updateOrder;

            foreach (var sceneName in activeSceneNames)
            {
                AddSceneByName(sceneName);
            }
        }

        private void AddSceneByName(string sceneName)
        {
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
                string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (sceneNameFromPath.Equals(sceneName, StringComparison.Ordinal))
                {
                    ActiveSceneBuildIndexes.Add(i);
                    return;
                }
            }

            DLogger.LogDevWarning($"AutoGameSystemAttribute: Scene name '{sceneName}' not found in build settings. Skipping.");
        }
    }
}