using System;

namespace NoSlimes.UnityUtils.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class ButtonMethodAttribute : Attribute
    {
        public string Label { get; }
        public ButtonMethodAttribute(string label = "")
        {
            Label = label;
        }
    }
}
