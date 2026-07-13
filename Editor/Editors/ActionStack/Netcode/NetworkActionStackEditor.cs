#if USE_NGO
using UnityEngine;
using NoSlimes.UnityUtils.Runtime.ActionStacks.Netcode.NGO;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace NoSlimes.UnityUtils.Editor.Editors.ActionStacks.Netcode.NGO
{
#if UNITY_EDITOR
    [CustomEditor(typeof(NetworkActionStack), true)]
    public class NetworkActionStackEditor : ActionStackEditor
    {
        private void OnEnable() => RefreshActions();

        private void RefreshActions()
        {
            if (EditorApplication.isPlaying)
                return;

            if (PrefabStageUtility.GetCurrentPrefabStage() == null)
                return;

            if (target != null)
            {
                ((NetworkActionStack)target).RefreshActions();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (PrefabStageUtility.GetCurrentPrefabStage() == null)
            {
                EditorGUILayout.HelpBox(
                    "Actions are only refreshed in the Prefab Stage.",
                    MessageType.Warning);
                return;
            }

            if (GUILayout.Button("Refresh Actions"))
            {
                RefreshActions();
            }
        }
    }
#endif
}
#endif