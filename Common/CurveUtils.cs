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

        public static void CubicBezier(Vector3 start, Vector3 end, Vector3 startTangent, Vector3 endTangent, int subdivisions, Vector3[] outPoints, float arcHeight = 0f)
        {
            if (outPoints == null || outPoints.Length != subdivisions + 1)
                throw new System.ArgumentException("outPoints must have length subdivisions + 1");

            for (int i = 0; i <= subdivisions; i++)
            {
                float t = i / (float)subdivisions;
                float u = 1f - t;

                // Cubic bezier
                Vector3 point = u * u * u * start +
                                3f * u * u * t * startTangent +
                                3f * u * t * t * endTangent +
                                t * t * t * end;

                // Lift along a parabolic arc from the start normal to the end normal
                float liftFactor = 4f * t * u; // peak at t = 0.5
                Vector3 lift = Vector3.Lerp(startTangent - start, endTangent - end, t).normalized * arcHeight * liftFactor;

                point += lift;

                outPoints[i] = point;
            }
        }

    }
}
