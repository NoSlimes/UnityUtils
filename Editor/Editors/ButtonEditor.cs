using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NoSlimes.UnityUtils.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace NoSlimes.UnityUtils.Editor.Editors
{
    public static class ButtonDrawer
    {
        private const int MaxDepth = 8;
        private const float FlowSpacing = 4f;
        private const BindingFlags MethodFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly Dictionary<Type, MethodInfo[]> buttonMethodsCache = new();
        private static readonly Dictionary<Type, bool> containerTypeCache = new();
        private static readonly Dictionary<Type, bool> inlineDrawerCache = new();

        public static void DrawButtons(UnityEngine.Object[] targets)
        {
            if (targets == null || targets.Length == 0)
            {
                return;
            }

            DrawTargetButtons(targets);
        }

        public static void DrawButtons(UnityEngine.Object[] targets, SerializedObject serializedObject)
        {
            if (targets == null || targets.Length == 0)
            {
                return;
            }

            bool invoked = DrawTargetButtons(targets);

            if (serializedObject != null && serializedObject.targetObject != null)
            {
                invoked |= DrawNestedSections(serializedObject);
            }

            if (invoked && serializedObject != null)
            {
                serializedObject.Update();
            }
        }

        public static void DrawNestedButtons(SerializedObject serializedObject)
        {
            if (serializedObject == null || serializedObject.targetObject == null)
            {
                return;
            }

            if (DrawNestedSections(serializedObject))
            {
                serializedObject.Update();
            }
        }

        public static MethodInfo[] GetButtonMethods(Type type)
        {
            if (type == null)
            {
                return Array.Empty<MethodInfo>();
            }

            if (!buttonMethodsCache.TryGetValue(type, out MethodInfo[] methods))
            {
                List<MethodInfo> found = new();
                for (Type current = type; current != null && current != typeof(object) && current != typeof(UnityEngine.Object); current = current.BaseType)
                {
                    foreach (MethodInfo method in current.GetMethods(MethodFlags | BindingFlags.DeclaredOnly))
                    {
                        if (IsButtonMethod(method))
                        {
                            found.Add(method);
                        }
                    }

                    if (current == typeof(MonoBehaviour) || current == typeof(ScriptableObject))
                    {
                        break;
                    }
                }

                methods = found.ToArray();
                buttonMethodsCache[type] = methods;
            }

            return methods;
        }

        public static string GetButtonLabel(MethodInfo method)
        {
            string label = method.GetCustomAttribute<ButtonMethodAttribute>()?.Label;
            return string.IsNullOrEmpty(label) ? method.Name : label;
        }

        public static float EstimateButtonWidth(string label)
        {
            return GUI.skin.button.CalcSize(new GUIContent(label)).x + 4f;
        }

        public static float FlowAvailableWidth()
        {
            return Mathf.Max(80f, EditorGUIUtility.currentViewWidth - 38f - EditorGUI.indentLevel * 15f);
        }

        public static List<int> PackRows(IReadOnlyList<string> labels, float availableWidth)
        {
            List<int> rows = new();
            int count = 0;
            float used = 0f;

            for (int i = 0; i < labels.Count; i++)
            {
                float width = EstimateButtonWidth(labels[i]);
                if (count > 0 && used + FlowSpacing + width > availableWidth)
                {
                    rows.Add(count);
                    count = 0;
                    used = 0f;
                }

                used += count == 0 ? width : FlowSpacing + width;
                count++;
            }

            if (count > 0)
            {
                rows.Add(count);
            }

            return rows;
        }

        public static int CountButtonRows(MethodInfo[] methods, float availableWidth)
        {
            if (methods == null || methods.Length == 0)
            {
                return 0;
            }

            List<string> labels = new(methods.Length);
            foreach (MethodInfo method in methods)
            {
                labels.Add(GetButtonLabel(method));
            }

            return PackRows(labels, availableWidth).Count;
        }

        public static void InvokeAtProperty(SerializedProperty property, MethodInfo template)
        {
            if (property == null || template == null)
            {
                return;
            }

            SerializedObject serializedObject = property.serializedObject;
            bool invoked = false;

            foreach (UnityEngine.Object root in serializedObject.targetObjects)
            {
                if (root == null)
                {
                    continue;
                }

                if (!TryParsePropertyPath(root.GetType(), property.propertyPath, out List<PathStep> steps))
                {
                    continue;
                }

                try
                {
                    invoked |= TryInvokeAtPath(root, steps, template);
                }
                catch (Exception e)
                {
                    Debug.LogException(Unwrap(e), root);
                }
            }

            if (invoked)
            {
                serializedObject.Update();
            }
        }

        private static bool DrawTargetButtons(UnityEngine.Object[] targets)
        {
            MethodInfo[] methods = GetButtonMethods(targets[0].GetType());
            if (methods.Length == 0)
            {
                return false;
            }

            return DrawFlowRows(methods, FlowAvailableWidth(), template =>
            {
                bool invoked = false;
                foreach (UnityEngine.Object target in targets)
                {
                    if (target == null)
                    {
                        continue;
                    }

                    try
                    {
                        invoked |= TryInvokeAtPath(target, Array.Empty<PathStep>(), template);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(Unwrap(e), target);
                    }
                }

                return invoked;
            });
        }

        private static bool DrawFlowRows(MethodInfo[] methods, float availableWidth, Func<MethodInfo, bool> onClick)
        {
            List<string> labels = new(methods.Length);
            List<float> widths = new(methods.Length);
            foreach (MethodInfo method in methods)
            {
                string label = GetButtonLabel(method);
                labels.Add(label);
                widths.Add(EstimateButtonWidth(label));
            }

            bool invoked = false;
            int cursor = 0;
            foreach (int rowCount in PackRows(labels, availableWidth))
            {
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < rowCount; i++)
                {
                    int index = cursor++;
                    if (GUILayout.Button(labels[index], GUILayout.MinWidth(widths[index]), GUILayout.ExpandWidth(true)) && onClick(methods[index]))
                    {
                        invoked = true;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            return invoked;
        }

        private static bool DrawNestedSections(SerializedObject serializedObject)
        {
            bool invoked = false;
            WalkState state = new();
            DrawContainerFields(serializedObject, serializedObject.targetObject, new List<PathStep>(), string.Empty, 0, state, ref invoked);
            return invoked;
        }

        private static void DrawContainerFields(SerializedObject serializedObject, object container, List<PathStep> steps, string path, int depth, WalkState state, ref bool invoked)
        {
            if (container == null || depth > MaxDepth)
            {
                return;
            }

            Type containerType = container.GetType();
            if (!containerType.IsValueType && !state.visited.Add(container))
            {
                return;
            }

            for (Type current = containerType; current != null && current != typeof(object) && current != typeof(UnityEngine.Object); current = current.BaseType)
            {
                foreach (FieldInfo field in current.GetFields(FieldFlags | BindingFlags.DeclaredOnly))
                {
                    if (!IsSerializedField(field))
                    {
                        continue;
                    }

                    DrawField(serializedObject, container, field, steps, path, depth, state, ref invoked);
                }

                if (current == typeof(MonoBehaviour) || current == typeof(ScriptableObject))
                {
                    break;
                }
            }
        }

        private static void DrawField(SerializedObject serializedObject, object container, FieldInfo field, List<PathStep> steps, string path, int depth, WalkState state, ref bool invoked)
        {
            object value;
            try
            {
                value = field.GetValue(container);
            }
            catch
            {
                return;
            }

            Type fieldType = field.FieldType;
            string childPath = string.IsNullOrEmpty(path) ? field.Name : path + "." + field.Name;

            steps.Add(new FieldStep(field));
            try
            {
                if (IsListType(fieldType, out _))
                {
                    if (value is not IList list)
                    {
                        return;
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        object element;
                        try
                        {
                            element = list[i];
                        }
                        catch
                        {
                            continue;
                        }

                        if (element == null)
                        {
                            continue;
                        }

                        steps.Add(new IndexStep(i));
                        try
                        {
                            DrawNode(serializedObject, element, steps, childPath + "[" + i + "]", depth + 1, state, ref invoked);
                        }
                        finally
                        {
                            steps.RemoveAt(steps.Count - 1);
                        }
                    }
                }
                else if (value != null)
                {
                    DrawNode(serializedObject, value, steps, childPath, depth + 1, state, ref invoked);
                }
            }
            finally
            {
                steps.RemoveAt(steps.Count - 1);
            }
        }

        private static void DrawNode(SerializedObject serializedObject, object value, List<PathStep> steps, string displayPath, int depth, WalkState state, ref bool invoked)
        {
            if (value == null)
            {
                return;
            }

            Type runtimeType = value.GetType();
            if (runtimeType == typeof(string) || typeof(UnityEngine.Object).IsAssignableFrom(runtimeType))
            {
                return;
            }

            if (!TypeCanContainButtons(runtimeType))
            {
                return;
            }

            if (GetButtonMethods(runtimeType).Length > 0 && !HasInlineDrawer(runtimeType))
            {
                DrawSection(serializedObject, steps, displayPath, runtimeType, ref invoked);
            }

            if (depth <= MaxDepth)
            {
                DrawContainerFields(serializedObject, value, steps, displayPath, depth, state, ref invoked);
            }
        }

        private static void DrawSection(SerializedObject serializedObject, List<PathStep> steps, string displayPath, Type instanceType, ref bool invoked)
        {
            MethodInfo[] methods = GetButtonMethods(instanceType);
            if (methods.Length == 0)
            {
                return;
            }

            List<PathStep> snapshot = new(steps);
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField(displayPath, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            try
            {
                if (DrawFlowRows(methods, FlowAvailableWidth(), template =>
                    {
                        InvokeAtPath(serializedObject, snapshot, template);
                        return true;
                    }))
                {
                    invoked = true;
                }
            }
            finally
            {
                EditorGUI.indentLevel--;
            }
        }

        private static void InvokeAtPath(SerializedObject serializedObject, IReadOnlyList<PathStep> steps, MethodInfo template)
        {
            bool invoked = false;

            foreach (UnityEngine.Object root in serializedObject.targetObjects)
            {
                if (root == null)
                {
                    continue;
                }

                try
                {
                    invoked |= TryInvokeAtPath(root, steps, template);
                }
                catch (Exception e)
                {
                    Debug.LogException(Unwrap(e), root);
                }
            }

            if (invoked)
            {
                serializedObject.Update();
            }
        }

        private static bool TryInvokeAtPath(UnityEngine.Object root, IReadOnlyList<PathStep> steps, MethodInfo template)
        {
            object node = root;
            List<object> parents = new();
            List<Action<object, object>> setters = new();

            foreach (PathStep step in steps)
            {
                if (node == null)
                {
                    return false;
                }

                switch (step)
                {
                    case FieldStep fieldStep:
                        FieldInfo field = ResolveField(node.GetType(), fieldStep.field);
                        if (field == null)
                        {
                            return false;
                        }

                        FieldInfo capturedField = field;
                        parents.Add(node);
                        setters.Add((parent, child) => capturedField.SetValue(parent, child));
                        node = field.GetValue(node);
                        break;

                    case IndexStep indexStep:
                        if (node is not IList list || indexStep.index < 0 || indexStep.index >= list.Count)
                        {
                            return false;
                        }

                        int capturedIndex = indexStep.index;
                        parents.Add(node);
                        setters.Add((parent, child) => ((IList)parent)[capturedIndex] = child);
                        node = list[capturedIndex];
                        break;

                    default:
                        return false;
                }
            }

            if (node == null)
            {
                return false;
            }

            MethodInfo method = ResolveMethod(node.GetType(), template);
            if (method == null)
            {
                return false;
            }

            Undo.RecordObject(root, "Invoke " + method.Name);
            method.Invoke(node, null);

            object child = node;
            for (int i = parents.Count - 1; i >= 0; i--)
            {
                setters[i](parents[i], child);
                child = parents[i];
            }

            EditorUtility.SetDirty(root);
            return true;
        }

        private static bool TryParsePropertyPath(Type rootType, string propertyPath, out List<PathStep> steps)
        {
            steps = new List<PathStep>();
            Type current = rootType;

            string[] tokens = propertyPath.Split('.');
            for (int i = 0; i < tokens.Length; i++)
            {
                if (tokens[i] == "Array" && i + 1 < tokens.Length && tokens[i + 1].StartsWith("data[", StringComparison.Ordinal) && tokens[i + 1].EndsWith("]", StringComparison.Ordinal))
                {
                    if (!int.TryParse(tokens[i + 1].Substring(5, tokens[i + 1].Length - 6), out int index))
                    {
                        return false;
                    }

                    steps.Add(new IndexStep(index));
                    current = GetElementType(current);
                    i++;
                }
                else
                {
                    FieldInfo field = FindField(current, tokens[i]);
                    if (field == null)
                    {
                        return false;
                    }

                    steps.Add(new FieldStep(field));
                    current = field.FieldType;
                }

                if (current == null)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsButtonMethod(MethodInfo method)
        {
            return method.GetCustomAttribute<ButtonMethodAttribute>() != null
                && !method.IsStatic
                && !method.IsAbstract
                && !method.IsGenericMethodDefinition
                && method.GetParameters().Length == 0;
        }

        private static bool IsSerializedField(FieldInfo field)
        {
            if (field.IsStatic || field.IsNotSerialized)
            {
                return false;
            }

            if (field.Name.Contains("k__BackingField"))
            {
                return false;
            }

            if (Attribute.IsDefined(field, typeof(HideInInspector)))
            {
                return false;
            }

            return field.IsPublic || Attribute.IsDefined(field, typeof(SerializeField));
        }

        private static bool IsListType(Type type, out Type elementType)
        {
            if (type.IsArray && type.GetArrayRank() == 1)
            {
                elementType = type.GetElementType();
                return true;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }

            elementType = null;
            return false;
        }

        private static Type GetElementType(Type listType)
        {
            return IsListType(listType, out Type elementType) ? elementType : null;
        }

        private static bool TypeCanContainButtons(Type type)
        {
            if (type == null)
            {
                return false;
            }

            if (!containerTypeCache.TryGetValue(type, out bool result))
            {
                result = CheckTypeCanContainButtons(type, new HashSet<Type>());
                containerTypeCache[type] = result;
            }

            return result;
        }

        private static bool CheckTypeCanContainButtons(Type type, HashSet<Type> inProgress)
        {
            if (GetButtonMethods(type).Length > 0)
            {
                return true;
            }

            if (type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal))
            {
                return false;
            }

            if (typeof(UnityEngine.Object).IsAssignableFrom(type) || typeof(Type).IsAssignableFrom(type))
            {
                return false;
            }

            if (IsListType(type, out Type elementType))
            {
                return elementType != null && (containerTypeCache.TryGetValue(elementType, out bool cached) ? cached : CheckTypeCanContainButtons(elementType, inProgress));
            }

            if (type.IsClass && !type.IsSerializable)
            {
                return false;
            }

            if (!inProgress.Add(type))
            {
                return false;
            }

            try
            {
                for (Type current = type; current != null && current != typeof(object) && current != typeof(ValueType) && current != typeof(UnityEngine.Object); current = current.BaseType)
                {
                    foreach (FieldInfo field in current.GetFields(FieldFlags | BindingFlags.DeclaredOnly))
                    {
                        if (!IsSerializedField(field))
                        {
                            continue;
                        }

                        Type fieldType = field.FieldType;
                        bool contained = containerTypeCache.TryGetValue(fieldType, out bool fieldCached)
                            ? fieldCached
                            : CheckTypeCanContainButtons(fieldType, inProgress);

                        if (contained)
                        {
                            return true;
                        }
                    }
                }
            }
            finally
            {
                inProgress.Remove(type);
            }

            return false;
        }

        private static bool HasInlineDrawer(Type type)
        {
            if (!inlineDrawerCache.TryGetValue(type, out bool hasDrawer))
            {
                hasDrawer = CheckHasInlineDrawer(type);
                inlineDrawerCache[type] = hasDrawer;
            }

            return hasDrawer;
        }

        private static bool CheckHasInlineDrawer(Type type)
        {
            foreach (Type drawer in TypeCache.GetTypesDerivedFrom<PropertyDrawer>())
            {
                if (drawer.IsAbstract || drawer.IsInterface)
                {
                    continue;
                }

                foreach (CustomAttributeData data in drawer.GetCustomAttributesData())
                {
                    if (data.AttributeType != typeof(CustomPropertyDrawer) || data.ConstructorArguments.Count == 0)
                    {
                        continue;
                    }

                    if (data.ConstructorArguments[0].Value is not Type drawerTarget)
                    {
                        continue;
                    }

                    bool useForChildren = data.ConstructorArguments.Count > 1 && data.ConstructorArguments[1].Value is bool forChildren && forChildren;
                    if (drawerTarget == type || (useForChildren && drawerTarget.IsAssignableFrom(type)))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static FieldInfo FindField(Type type, string name)
        {
            for (Type current = type; current != null && current != typeof(object); current = current.BaseType)
            {
                FieldInfo field = current.GetField(name, FieldFlags | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    return field;
                }
            }

            return null;
        }

        private static FieldInfo ResolveField(Type runtimeType, FieldInfo template)
        {
            if (template.DeclaringType != null && template.DeclaringType.IsAssignableFrom(runtimeType))
            {
                return template;
            }

            return FindField(runtimeType, template.Name);
        }

        private static MethodInfo ResolveMethod(Type runtimeType, MethodInfo template)
        {
            foreach (MethodInfo candidate in runtimeType.GetMethods(MethodFlags))
            {
                if (candidate.Name == template.Name
                    && candidate.GetParameters().Length == 0
                    && !candidate.IsStatic
                    && !candidate.IsAbstract
                    && !candidate.IsGenericMethodDefinition
                    && candidate.GetCustomAttribute<ButtonMethodAttribute>() != null)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static Exception Unwrap(Exception e)
        {
            return e is TargetInvocationException invocation && invocation.InnerException != null ? invocation.InnerException : e;
        }

        private sealed class WalkState
        {
            public readonly HashSet<object> visited = new(ReferenceEqualityComparer.Instance);
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new();

            public new bool Equals(object left, object right)
            {
                return ReferenceEquals(left, right);
            }

            public int GetHashCode(object value)
            {
                return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value);
            }
        }

        public abstract class PathStep
        {
        }

        public sealed class FieldStep : PathStep
        {
            public readonly FieldInfo field;

            public FieldStep(FieldInfo field)
            {
                this.field = field;
            }
        }

        public sealed class IndexStep : PathStep
        {
            public readonly int index;

            public IndexStep(int index)
            {
                this.index = index;
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
            ButtonDrawer.DrawButtons(targets, serializedObject);
        }
    }

    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    public class ScriptableObjectButtonEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            ButtonDrawer.DrawButtons(targets, serializedObject);
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
            ButtonDrawer.DrawButtons(targets, serializedObject);
        }
    }
#endif

}
