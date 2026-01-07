using NoSlimes.UnityUtils.Common.Attributes;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(InlineInspectorAttribute), true)]
public class InlineInspectorDrawer : PropertyDrawer
{
    private static Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        string key = property.propertyPath;
        if (!foldouts.ContainsKey(key)) foldouts[key] = false;

        float y = position.y;
        Rect fieldRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);

        // Handle arrays / lists
        if (property.isArray && property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.PropertyField(fieldRect, property, label, false);
            y += EditorGUIUtility.singleLineHeight + 2;

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                int count = property.arraySize;
                for (int i = 0; i < count; i++)
                {
                    SerializedProperty element = property.GetArrayElementAtIndex(i);
                    string elementKey = key + ".Array.data[" + i + "]";
                    if (!foldouts.ContainsKey(elementKey)) foldouts[elementKey] = false;

                    float elementHeight = EditorGUI.GetPropertyHeight(element, true);
                    Rect elementRect = new Rect(position.x, y, position.width, elementHeight);

                    // ObjectReference element
                    if (element.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        element.objectReferenceValue = EditorGUI.ObjectField(elementRect, $"Element {i}", element.objectReferenceValue, typeof(Object), true);
                        y += elementHeight + 2;

                        if (element.objectReferenceValue != null)
                        {
                            string foldoutLabel = $"{element.displayName} Fields";
                            Rect foldoutRect = new Rect(position.x + 15, y, position.width - 15, EditorGUIUtility.singleLineHeight);

                            // Draw dark background behind the foldout header
                            EditorGUI.DrawRect(foldoutRect, new Color(0.15f, 0.15f, 0.15f, 0.5f));
                            foldouts[elementKey] = EditorGUI.Foldout(foldoutRect, foldouts[elementKey], foldoutLabel, true);
                            y += EditorGUIUtility.singleLineHeight + 2;

                            if (foldouts[elementKey])
                            {
                                float boxStartY = y - 2;
                                float contentHeight = CalculateObjectFieldsHeight(element.objectReferenceValue);
                                EditorGUI.DrawRect(new Rect(position.x + 15, boxStartY, position.width - 15, contentHeight), new Color(0.2f, 0.2f, 0.2f, 0.25f));
                                y = DrawObjectFields(element.objectReferenceValue, position.x + 20, y, position.width - 30);
                            }
                        }
                    }
                    else // Serializable element
                    {
                        string foldoutLabel = $"Element {i} Fields";
                        Rect foldoutRect = new Rect(position.x + 15, y, position.width - 15, EditorGUIUtility.singleLineHeight);

                        // Background behind foldout header
                        EditorGUI.DrawRect(foldoutRect, new Color(0.15f, 0.15f, 0.15f, 0.5f));
                        foldouts[elementKey] = EditorGUI.Foldout(foldoutRect, foldouts[elementKey], foldoutLabel, true);
                        y += elementHeight + 2;

                        if (foldouts[elementKey])
                        {
                            float boxStartY = y - 2;
                            float contentHeight = CalculateSerializableFieldsHeight(element);
                            EditorGUI.DrawRect(new Rect(position.x + 15, boxStartY, position.width - 15, contentHeight), new Color(0.2f, 0.2f, 0.2f, 0.25f));
                            y = DrawSerializableFields(element, position.x + 20, y, position.width - 30);
                        }
                    }
                }
                EditorGUI.indentLevel--;
            }
        }
        // Single ObjectReference
        else if (property.propertyType == SerializedPropertyType.ObjectReference)
        {
            property.objectReferenceValue = EditorGUI.ObjectField(fieldRect, label, property.objectReferenceValue, typeof(Object), true);
            y += EditorGUIUtility.singleLineHeight + 2;

            if (property.objectReferenceValue != null)
            {
                string foldoutLabel = $"{property.displayName} Fields";
                Rect foldoutRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);

                // Dark background behind foldout header
                EditorGUI.DrawRect(foldoutRect, new Color(0.15f, 0.15f, 0.15f, 0.5f));
                foldouts[key] = EditorGUI.Foldout(foldoutRect, foldouts[key], foldoutLabel, true);
                y += EditorGUIUtility.singleLineHeight + 2;

                if (foldouts[key])
                {
                    float boxStartY = y - 2;
                    float contentHeight = CalculateObjectFieldsHeight(property.objectReferenceValue);
                    EditorGUI.DrawRect(new Rect(position.x + 5, boxStartY, position.width - 10, contentHeight), new Color(0.2f, 0.2f, 0.2f, 0.25f));
                    y = DrawObjectFields(property.objectReferenceValue, position.x + 10, y, position.width - 20);
                }
            }
        }
        // Serializable field
        else
        {
            string foldoutLabel = $"{property.displayName} Fields";
            Rect foldoutRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);

            // Background behind foldout header
            EditorGUI.DrawRect(foldoutRect, new Color(0.15f, 0.15f, 0.15f, 0.5f));
            foldouts[key] = EditorGUI.Foldout(foldoutRect, foldouts[key], foldoutLabel, true);
            y += EditorGUIUtility.singleLineHeight + 2;

            if (foldouts[key])
            {
                float boxStartY = y - 2;
                float contentHeight = CalculateSerializableFieldsHeight(property);
                EditorGUI.DrawRect(new Rect(position.x + 5, boxStartY, position.width - 10, contentHeight), new Color(0.2f, 0.2f, 0.2f, 0.25f));
                y = DrawSerializableFields(property, position.x + 10, y, position.width - 20);
            }
        }

        EditorGUI.EndProperty();
    }

    private float CalculateObjectFieldsHeight(Object obj)
    {
        float y = 0f;
        SerializedObject so = new SerializedObject(obj);
        SerializedProperty prop = so.GetIterator();
        prop.NextVisible(true);
        while (prop.NextVisible(false))
            y += EditorGUI.GetPropertyHeight(prop, true) + 2;
        return y;
    }

    private float CalculateSerializableFieldsHeight(SerializedProperty property)
    {
        float y = 0f;
        SerializedProperty prop = property.Copy();
        SerializedProperty end = prop.GetEndProperty(true);
        prop.NextVisible(true);
        while (!SerializedProperty.EqualContents(prop, end))
        {
            y += EditorGUI.GetPropertyHeight(prop, true) + 2;
            prop.NextVisible(false);
        }
        return y;
    }

    private float DrawObjectFields(Object obj, float x, float y, float width)
    {
        SerializedObject so = new SerializedObject(obj);
        SerializedProperty prop = so.GetIterator();
        prop.NextVisible(true); // skip script

        while (prop.NextVisible(false))
        {
            float h = EditorGUI.GetPropertyHeight(prop, true);
            EditorGUI.PropertyField(new Rect(x, y, width, h), prop, true);
            y += h + 2;
        }

        so.ApplyModifiedProperties();
        return y;
    }

    private float DrawSerializableFields(SerializedProperty property, float x, float y, float width)
    {
        SerializedProperty prop = property.Copy();
        SerializedProperty end = prop.GetEndProperty(true);
        prop.NextVisible(true);

        while (!SerializedProperty.EqualContents(prop, end))
        {
            float h = EditorGUI.GetPropertyHeight(prop, true);
            EditorGUI.PropertyField(new Rect(x, y, width, h), prop, true);
            y += h + 2;
            prop.NextVisible(false);
        }

        return y;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight + 2;
        string key = property.propertyPath;
        if (!foldouts.ContainsKey(key)) foldouts[key] = false;

        float totalHeight = lineHeight; // Start with the main property line

        // Arrays / Lists
        if (property.isArray && property.propertyType != SerializedPropertyType.String)
        {
            if (!property.isExpanded)
                return totalHeight; // Collapsed array, just the array line

            int count = property.arraySize;
            for (int i = 0; i < count; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                string elementKey = key + ".Array.data[" + i + "]";
                if (!foldouts.ContainsKey(elementKey)) foldouts[elementKey] = false;

                // Add height for element itself
                totalHeight += EditorGUI.GetPropertyHeight(element, true) + 2;

                // Always reserve space for the foldout line for ObjectReferences
                if (element.propertyType == SerializedPropertyType.ObjectReference && element.objectReferenceValue != null)
                {
                    totalHeight += lineHeight; // Show Fields foldout line

                    // Add child fields only if element foldout is expanded
                    if (foldouts[elementKey])
                    {
                        SerializedObject so = new SerializedObject(element.objectReferenceValue);
                        SerializedProperty prop = so.GetIterator();
                        prop.NextVisible(true);
                        while (prop.NextVisible(false))
                            totalHeight += EditorGUI.GetPropertyHeight(prop, true) + 2;
                    }
                }
                else if (foldouts[elementKey])
                {
                    // Serializable elements: add child fields if expanded
                    SerializedProperty prop = element.Copy();
                    SerializedProperty end = prop.GetEndProperty(true);
                    prop.NextVisible(true);
                    while (!SerializedProperty.EqualContents(prop, end))
                        totalHeight += EditorGUI.GetPropertyHeight(prop, true) + 2;
                }
            }

            return totalHeight;
        }

        // Single ObjectReference
        if (property.propertyType == SerializedPropertyType.ObjectReference)
        {
            if (property.objectReferenceValue != null)
            {
                totalHeight += lineHeight; // Always reserve Show Fields foldout line

                if (foldouts[key])
                {
                    SerializedObject so = new SerializedObject(property.objectReferenceValue);
                    SerializedProperty prop = so.GetIterator();
                    prop.NextVisible(true);
                    while (prop.NextVisible(false))
                        totalHeight += EditorGUI.GetPropertyHeight(prop, true) + 2;
                }
            }

            return totalHeight;
        }

        // Serializable class/struct
        if (foldouts[key])
        {
            SerializedProperty prop = property.Copy();
            SerializedProperty end = prop.GetEndProperty(true);
            prop.NextVisible(true);
            while (!SerializedProperty.EqualContents(prop, end))
                totalHeight += EditorGUI.GetPropertyHeight(prop, true) + 2;
        }

        return totalHeight;
    }

}
