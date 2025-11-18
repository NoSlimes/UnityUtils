using NoSlimes.Utils.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace NoSlimes.Utils.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ReadOnlyFieldAttribute))]
    internal class ReadOnlyFieldAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }
    }
}