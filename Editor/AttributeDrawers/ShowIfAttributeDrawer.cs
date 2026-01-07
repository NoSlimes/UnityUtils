using NoSlimes.UnityUtils.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace NoSlimes.UnityUtils.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    internal class ShowIfAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute attr = (ShowIfAttribute)attribute;

            SerializedProperty conditionProp = property.serializedObject.FindProperty(attr.ConditionFieldName);

            if (conditionProp != null && conditionProp.propertyType == SerializedPropertyType.Boolean)
            {
                bool show = conditionProp.boolValue;
                if (attr.Inverse) show = !show;

                if (show)
                    EditorGUI.PropertyField(position, property, label, true);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
                if (conditionProp == null)
                    Debug.LogWarning($"ShowIf: Boolean field '{attr.ConditionFieldName}' not found.");
                else
                    Debug.LogWarning($"ShowIf: Field '{attr.ConditionFieldName}' is not a boolean.");
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute attr = (ShowIfAttribute)attribute;
            SerializedProperty conditionProp = property.serializedObject.FindProperty(attr.ConditionFieldName);

            bool show = conditionProp != null && conditionProp.propertyType == SerializedPropertyType.Boolean
                        ? conditionProp.boolValue
                        : true; 

            if (attr.Inverse) show = !show;

            return show ? EditorGUI.GetPropertyHeight(property, label, true) : 0f;
        }
    }
}