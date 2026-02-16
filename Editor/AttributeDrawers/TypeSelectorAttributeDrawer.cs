using System;
using System.Collections.Generic;
using System.Linq;
using NoSlimes.UnityUtils.Common.Attributes;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TypeSelectorAttribute))]
public class TypeSelectorAttributeDrawer : PropertyDrawer
{
    private static readonly Dictionary<Type, Type[]> typeCache = new Dictionary<Type, Type[]>();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        TypeSelectorAttribute typeAttr = (TypeSelectorAttribute)attribute;

        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "Use TypeSelector with string.");
            return;
        }

        if (!typeCache.TryGetValue(typeAttr.BaseType, out Type[] types))
        {
            types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); } 
                })
                .Where(t => typeAttr.BaseType.IsAssignableFrom(t) && (typeAttr.IncludeAbstract || !t.IsAbstract))
                .OrderBy(t => t.Name)
                .ToArray();

            typeCache[typeAttr.BaseType] = types;
        }

        int currentIndex = Array.FindIndex(types, t => t.FullName == property.stringValue);
        if (currentIndex < 0) currentIndex = 0;

        string[] typeNames = types.Select(t => t.Name).ToArray();
        int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, typeNames);

        if (selectedIndex >= 0 && selectedIndex < types.Length)
            property.stringValue = types[selectedIndex].FullName;
    }
}
