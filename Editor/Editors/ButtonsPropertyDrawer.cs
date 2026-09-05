using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace NoSlimes.UnityUtils.Editor.Editors
{
    public abstract class ButtonsPropertyDrawer<T> : PropertyDrawer
    {
        private const float Spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float y = position.y;
            float propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, propertyHeight), property, label, true);
            y += propertyHeight + Spacing;

            MethodInfo[] methods = ButtonDrawer.GetButtonMethods(typeof(T));
            if (methods.Length > 0)
            {
                List<string> labels = new(methods.Length);
                List<float> widths = new(methods.Length);
                foreach (MethodInfo method in methods)
                {
                    string buttonLabel = ButtonDrawer.GetButtonLabel(method);
                    labels.Add(buttonLabel);
                    widths.Add(ButtonDrawer.EstimateButtonWidth(buttonLabel));
                }

                int cursor = 0;
                foreach (int rowCount in ButtonDrawer.PackRows(labels, PackWidth()))
                {
                    float total = 0f;
                    for (int i = 0; i < rowCount; i++)
                    {
                        total += widths[cursor + i];
                    }

                    total += (rowCount - 1) * Spacing;
                    float extra = position.width > total ? (position.width - total) / rowCount : 0f;

                    float x = position.x;
                    for (int i = 0; i < rowCount; i++)
                    {
                        int index = cursor++;
                        float width = widths[index] + extra;
                        if (GUI.Button(new Rect(x, y, width, EditorGUIUtility.singleLineHeight), labels[index]))
                        {
                            ButtonDrawer.InvokeAtProperty(property, methods[index]);
                        }

                        x += width + Spacing;
                    }

                    y += EditorGUIUtility.singleLineHeight + Spacing;
                }
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUI.GetPropertyHeight(property, label, true) + Spacing;
            height += ButtonDrawer.CountButtonRows(ButtonDrawer.GetButtonMethods(typeof(T)), PackWidth()) * (EditorGUIUtility.singleLineHeight + Spacing);
            return height;
        }

        private static float PackWidth()
        {
            return Mathf.Max(80f, EditorGUIUtility.currentViewWidth - 40f);
        }
    }
}
