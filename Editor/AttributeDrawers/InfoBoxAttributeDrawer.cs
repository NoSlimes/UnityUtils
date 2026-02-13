using NoSlimes.UnityUtils.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace NoSlimes.UnityUtils.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    internal class InfoBoxAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float helpBoxHeight = EditorStyles.helpBox.CalcHeight(
                new GUIContent(((InfoBoxAttribute)attribute).Message),
                EditorGUIUtility.currentViewWidth
            );

            float propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);

            return helpBoxHeight + 4 + propertyHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InfoBoxAttribute attr = (InfoBoxAttribute)attribute;

            MessageType messageType = attr.MessageType switch
            {
                InfoBoxAttribute.MessageSeverity.Info => MessageType.Info,
                InfoBoxAttribute.MessageSeverity.Warning => MessageType.Warning,
                InfoBoxAttribute.MessageSeverity.Error => MessageType.Error,
                _ => MessageType.Info
            };

            float helpBoxHeight = EditorStyles.helpBox.CalcHeight(
                new GUIContent(attr.Message),
                position.width
            );

            Rect helpBoxRect = new Rect(position.x, position.y, position.width, helpBoxHeight);
            EditorGUI.HelpBox(helpBoxRect, attr.Message, messageType);

            Rect propertyRect = new Rect(
                position.x,
                helpBoxRect.yMax + 4,
                position.width,
                EditorGUI.GetPropertyHeight(property, label, true)
            );

            EditorGUI.PropertyField(propertyRect, property, label, true);
        }
    }
}
