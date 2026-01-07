using UnityEngine;
using UnityEngine.UIElements;

namespace NoSlimes.UnityUtils.Common.VisualElements
{
    public class ToggleButton : VisualElement
    {
        public bool Value { get; private set; }

        private readonly VisualElement labelContainer;
        private readonly VisualElement indicator;
        private readonly Label label;

        public event System.Action<bool> OnValueChanged;

        private readonly Color activeColor = new(0.5f, 0.9f, 0.5f); // gentle green
        private readonly Color inactiveColor = new(0.9f, 0.5f, 0.5f); // gentle red
        private readonly Color backgroundColor = new(0.3f, 0.3f, 0.3f); // gray

        public ToggleButton(string text, bool initialValue = false)
        {
            Value = initialValue;

            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Stretch;
            style.justifyContent = Justify.FlexStart;
            style.height = 30;
            style.backgroundColor = backgroundColor;
            style.borderBottomLeftRadius = 6;
            style.borderTopLeftRadius = 6;
            style.borderBottomRightRadius = 6;
            style.borderTopRightRadius = 6;
            style.marginBottom = 6;
            style.overflow = Overflow.Hidden;

            labelContainer = new VisualElement
            {
                style =
                    {
                        flexGrow = 1,
                        paddingLeft = 6,
                        paddingRight = 6,
                        flexDirection = FlexDirection.Row,
                        justifyContent = Justify.FlexStart,
                        alignItems = Align.Center
                    }
            };
            Add(labelContainer);

            label = new Label(text)
            {
                style =
                    {
                        unityFontStyleAndWeight = FontStyle.Bold,
                        color = Color.white,
                        unityTextAlign = TextAnchor.MiddleLeft,
                        flexShrink = 0
                    }
            };
            labelContainer.Add(label);

            indicator = new VisualElement
            {
                style =
            {
                width = Length.Percent(10),
                backgroundColor = Value ? activeColor : inactiveColor,
                alignSelf = Align.Stretch,
                borderBottomRightRadius = 6,
                borderTopRightRadius = 6,
                borderBottomLeftRadius = 0,
                borderTopLeftRadius = 0
            }
            };
            Add(indicator);

            RegisterCallback<MouseDownEvent>(evt => Toggle());
        }

        public void Toggle()
        {
            Value = !Value;
            indicator.style.backgroundColor = Value ? activeColor : inactiveColor;
            OnValueChanged?.Invoke(Value);
        }

        public void SetValue(bool value)
        {
            Value = value;
            indicator.style.backgroundColor = Value ? activeColor : inactiveColor;
        }
    }
}