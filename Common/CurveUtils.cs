using UnityEngine;

namespace NoSlimes.UnityUtils.Common
{
    public static class CurveUtils
    {
        public static void QuadraticArc(Vector3 start, Vector3 end, Vector3 upReference, float arcHeight, int subdivisions, Vector3[] output)
        {
            Vector3 mid = (start + end) * 0.5f;
            Vector3 up = (mid - upReference).normalized;
            Vector3 control = mid + (up * arcHeight);

            for (int i = 0; i <= subdivisions; i++)
            {
                float t = i / (float)subdivisions;
                output[i] = BezierUtils.QuadraticBezier(start, control, end, t);
            }
        }
    }
}
