using UnityEngine;

namespace NoSlimes.UnityUtils.Common
{
    public static class BezierUtils
    {
        public static Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }
    }
}