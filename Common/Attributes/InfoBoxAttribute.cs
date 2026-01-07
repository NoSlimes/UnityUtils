using UnityEngine;

namespace NoSlimes.UnityUtils.Common.Attributes
{
    public class InfoBoxAttribute : PropertyAttribute
    {
        public enum MessageSeverity
        {
            Info,
            Warning,
            Error
        }

        public string Message { get; }
        public MessageSeverity MessageType { get; }
        public InfoBoxAttribute(string message, MessageSeverity type = MessageSeverity.Info)
        {
            Message = message;
            MessageType = type;
        }
    }
}
