using NoSlimes.UnityUtils.Common.Attributes;
using UnityEditor;
using UnityEngine;

namespace NoSlimes.UnityUtils.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    internal class InfoBoxAttributeDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            return EditorGUIUtility.singleLineHeight * 2 + 4;
        }

        public override void OnGUI(Rect position)
        {
            InfoBoxAttribute attr = (InfoBoxAttribute)attribute;

            MessageType messageType = attr.MessageType switch
            {
                InfoBoxAttribute.MessageSeverity.Info => MessageType.Info,
                InfoBoxAttribute.MessageSeverity.Warning => MessageType.Warning,
                InfoBoxAttribute.MessageSeverity.Error => MessageType.Error,
                _ => MessageType.Info
            };

            EditorGUI.HelpBox(position, attr.Message, messageType);
        }
    }
}
