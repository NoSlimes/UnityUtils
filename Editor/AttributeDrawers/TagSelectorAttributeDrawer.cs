using System;
using System.Linq;
using NoSlimes.UnityUtils.Common.Attributes;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomPropertyDrawer(typeof(TagSelectorAttribute))]
public class TagSelectorAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "Use TagSelector with string.");
            return;
        }

        string[] tags = InternalEditorUtility.tags;

        if (tags == null || tags.Length == 0)
        {
            EditorGUI.LabelField(position, label.text, "No tags defined.");
            return;
        }

        // Missing values (e.g. tag deleted after assignment) are prepended so
        // merely viewing the inspector never silently overwrites them.
        string current = property.stringValue;
        int currentIndex = Array.IndexOf(tags, current);

        string[] shown = currentIndex >= 0
            ? tags
            : new[] { string.IsNullOrEmpty(current) ? "<missing>" : current }.Concat(tags).ToArray();

        int picked = EditorGUI.Popup(position, label.text, currentIndex >= 0 ? currentIndex : 0, shown);

        if (picked != (currentIndex >= 0 ? currentIndex : 0))
            property.stringValue = shown[picked];
    }
}
