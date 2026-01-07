using NoSlimes.UnityUtils.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace NoSlimes.UnityUtils.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    internal class InfoBoxAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            InfoBoxAttribute attr = (InfoBoxAttribute)attribute;
            Rect infoBoxRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight * 2);

            var messageType = attr.MessageType switch
            {
                InfoBoxAttribute.MessageSeverity.Info => MessageType.Info,
                InfoBoxAttribute.MessageSeverity.Warning => MessageType.Warning,
                InfoBoxAttribute.MessageSeverity.Error => MessageType.Error,
                _ => MessageType.Info
            };


            EditorGUI.HelpBox(infoBoxRect, attr.Message, messageType);
            Rect propertyRect = new Rect(position.x, position.y + infoBoxRect.height + 4, position.width, EditorGUI.GetPropertyHeight(property, label, true));
            EditorGUI.PropertyField(propertyRect, property, label, true);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            InfoBoxAttribute attr = (InfoBoxAttribute)attribute;
            float infoBoxHeight = EditorGUIUtility.singleLineHeight * 2 + 4; // InfoBox height + spacing
            float propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);
            return infoBoxHeight + propertyHeight;
        }
    }
}
