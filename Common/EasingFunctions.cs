using UnityEngine;

namespace NoSlimes.UnityUtils.Common
{
    public struct EasingFunctions
    {
        public static float EaseIn(float t)
        {
            return t * t;
        }

        public static float EaseInBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;

            return (c3 * t * t * t) - (c1 * t * t);
        }

        public static float EaseInElastic(float t)
        {
            float c4 = 2 * Mathf.PI / 3;

            return t == 0
                ? 0
                : Mathf.Approximately(t, 1)
                ? 1
                : -Mathf.Pow(2, (10 * t) - 10) * Mathf.Sin(((t * 10) - 10.75f) * c4);
        }

        public static float EaseInBounce(float t)
        {
            return 1 - EaseOutBounce(1 - t);
        }

        public static float EaseOut(float t)
        {
            return 1 - ((1 - t) * (1 - t));
        }

        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;

            return 1 + (c3 * Mathf.Pow(t - 1, 3)) + (c1 * Mathf.Pow(t - 1, 2));
        }

        public static float EaseOutElastic(float t)
        {
            const float c4 = 2f * Mathf.PI / 3f;

            return t == 0
                ? 0
                : Mathf.Approximately(t, 1)
                ? 1
                : (Mathf.Pow(2f, -10f * t) * Mathf.Sin(((t * 10f) - 0.75f) * c4)) + 1f;
        }

        public static float EaseOutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1 / d1)
            {
                return n1 * t * t;
            }
            else if (t < 2 / d1)
            {
                return (n1 * (t -= 1.5f / d1) * t) + 0.75f;
            }
            else if (t < 2.5 / d1)
            {
                return (n1 * (t -= 2.25f / d1) * t) + 0.9375f;
            }
            else
            {
                return (n1 * (t -= 2.625f / d1) * t) + 0.984375f;
            }
        }

        public static float EaseInOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;
            float t2 = t - 1f;
            return t < 0.5
                ? t * t * 2 * (((c2 + 1) * t * 2) - c2)
                : (t2 * t2 * 2 * (((c2 + 1) * t2 * 2) + c2)) + 1;
        }

        public static float EaseInOutElastic(float x)
        {
            float c5 = 2 * Mathf.PI / 4.5f;

            return x == 0
                ? 0
                : Mathf.Approximately(x, 1)
                ? 1
                : x < 0.5f
                ? -Mathf.Pow(2, (20 * x) - 10) * Mathf.Sin(((20 * x) - 11.125f) * c5) / 2
                : (Mathf.Pow(2, (-20 * x) + 10) * Mathf.Sin(((20 * x) - 11.125f) * c5) / 2) + 1;
        }

        public static float EaseInOutBounce(float t)
        {
            return t < 0.5f
                ? (1 - EaseOutBounce(1 - (2 * t))) / 2
                : (1 - EaseOutBounce((2 * t) - 1)) / 2;
        }

        public static float EaseOutExponential(float t)
        {
            return Mathf.Approximately(t, 1) ? 1 : 1 - Mathf.Pow(2, -10 * t);
        }

        public static float EaseInExponential(float t)
        {
            return t == 0 ? 0 : Mathf.Pow(2, (10 * t) - 10);
        }

        public static float EaseInOutExponential(float t)
        {
            return t == 0
                ? 0
                : Mathf.Approximately(t, 1)
                ? 1
                : t < 0.5f ? Mathf.Pow(2, (20 * t) - 10) / 2
                : (2 - Mathf.Pow(2, (-20 * t) + 10)) / 2;
        }
    }
}