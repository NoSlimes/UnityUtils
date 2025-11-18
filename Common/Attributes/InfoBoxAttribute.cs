using UnityEditor;
using UnityEngine;

namespace NoSlimes.Utils.Common.Attributes
{
    public class InfoBoxAttribute : PropertyAttribute
    {
        public string Message { get; }
        public MessageType MessageType { get; }
        public InfoBoxAttribute(string message, MessageType type = MessageType.Info)
        {
            Message = message;
            MessageType = type;
        }
    }
}
