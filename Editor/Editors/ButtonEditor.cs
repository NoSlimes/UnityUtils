using System.Collections.Generic;
using System.Reflection;
using NoSlimes.UnityUtils.Common.Attributes;
using NoSlimes.UnityUtils.Editor.Editors;
using UnityEditor;
using UnityEngine;

namespace NoSlimes.UnityUtils.Editor.Editors
{
    public static class ButtonDrawer
    {
        public static void DrawButtons(UnityEngine.Object[] targets)
        {
            if (targets == null || targets.Length == 0) return;

            var type = targets[0].GetType();
            var methodGroups = new List<MethodInfo>();

            var currentType = type;
            while (currentType != null && currentType != typeof(MonoBehaviour) && currentType != typeof(ScriptableObject))
            {
                var methods = currentType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                methodGroups.AddRange(methods);
                currentType = currentType.BaseType;
            }

            foreach (var method in methodGroups)
            {
                var attr = method.GetCustomAttribute<ButtonMethodAttribute>();
                if (attr == null) continue;

                string label = string.IsNullOrEmpty(attr.Label) ? method.Name : attr.Label;

                if (GUILayout.Button(label))
                {
                    foreach (var t in targets)
                    {
                        method.Invoke(t, null);
                        EditorUtility.SetDirty(t); 
                    }
                }
            }
        }
    }

    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class MonoBehaviourButtonEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ButtonDrawer.DrawButtons(targets);
        }
    }

    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    public class ScriptableObjectButtonEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ButtonDrawer.DrawButtons(targets);
        }
    }

#if USE_NGO
    [CustomEditor(typeof(Unity.Netcode.NetworkBehaviour), true)]
    [CanEditMultipleObjects]
    public class NetworkBehaviourButtonEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI(); // This draws the default NetworkBehaviour fields
            ButtonDrawer.DrawButtons(targets);
        }
    }
#endif

}