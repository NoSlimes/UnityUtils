using NoSlimes.Utils.Editor.Attributes;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NoSlimes.Utils.Editor.Editors
{
    /// <summary>
    /// Generic editor that draws buttons for methods with [Button] attributes.
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class ButtonEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            DrawButtons(target);
        }

        private void DrawButtons(object targetObject)
        {
            var methods = targetObject.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
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
}