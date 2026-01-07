using NoSlimes.UnityUtils.Common.Attributes;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NoSlimes.UnityUtils.Editor.Editors
{
    public static class ButtonDrawer
    {
        public static void DrawButtons(object targetObject)
        {
            // Get all methods (Public, Private, Instance)
            MethodInfo[] methods = targetObject.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (MethodInfo method in methods)
            {
                var buttonAttr = method.GetCustomAttribute<ButtonMethodAttribute>();
                if (buttonAttr != null)
                {
                    string label = string.IsNullOrEmpty(buttonAttr.Label) ? method.Name : buttonAttr.Label;

                    if (GUILayout.Button(label))
                    {
                        method.Invoke(targetObject, null);
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
            ButtonDrawer.DrawButtons(target);
        }
    }

    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    public class ScriptableObjectButtonEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ButtonDrawer.DrawButtons(target);
        }
    }
}