using UnityEngine;

namespace NoSlimes.Utils.Editor.Attributes
{
    /// <summary>
    /// Shows a property in the Inspector only if the specified boolean field is true (or false if inverse).
    /// </summary>
    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionFieldName { get; }
        public bool Inverse { get; }

        public ShowIfAttribute(string conditionFieldName, bool inverse = false)
        {
            ConditionFieldName = conditionFieldName;
            Inverse = inverse;
        }
    }
}