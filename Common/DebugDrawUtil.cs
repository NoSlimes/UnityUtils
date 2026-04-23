using UnityEngine;

namespace NoSlimes.UnityUtils.Common
{
    public static class DebugDrawUtil
    {
#if DEBUG
        public static void DrawCapsule(Vector3 start, Vector3 end, float radius, Color color, float duration)
        {
            Debug.DrawLine(start, end, color, duration);
            DrawSphere(start, radius, color, duration);
            DrawSphere(end, radius, color, duration);
        }

        public static void DrawBox(Vector3 center, Quaternion rotation, Vector3 halfExtents, Color color, float duration)
        {
            Vector3 right = rotation * Vector3.right * halfExtents.x;
            Vector3 up = rotation * Vector3.up * halfExtents.y;
            Vector3 forward = rotation * Vector3.forward * halfExtents.z;

            Vector3[] c =
            {
                center + right + up + forward, center + right + up - forward,
                center + right - up + forward, center + right - up - forward,
                center - right + up + forward, center - right + up - forward,
                center - right - up + forward, center - right - up - forward
            };

            Debug.DrawLine(c[0], c[1], color, duration); Debug.DrawLine(c[0], c[2], color, duration);
            Debug.DrawLine(c[0], c[4], color, duration); Debug.DrawLine(c[7], c[3], color, duration);
            Debug.DrawLine(c[7], c[5], color, duration); Debug.DrawLine(c[7], c[6], color, duration);
            Debug.DrawLine(c[1], c[3], color, duration); Debug.DrawLine(c[1], c[5], color, duration);
            Debug.DrawLine(c[2], c[3], color, duration); Debug.DrawLine(c[2], c[6], color, duration);
            Debug.DrawLine(c[4], c[5], color, duration); Debug.DrawLine(c[4], c[6], color, duration);
        }

        public static void DrawSphere(Vector3 center, float radius, Color color, float duration, int segments = 12)
        {
            float step = (Mathf.PI * 2f) / segments;
            Vector3 prevX = center + new Vector3(radius, 0, 0);
            Vector3 prevY = center + new Vector3(0, radius, 0);
            Vector3 prevZ = center + new Vector3(0, 0, radius);

            for (int i = 1; i <= segments; i++)
            {
                float a = i * step;
                Vector3 nextX = center + new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0);
                Vector3 nextY = center + new Vector3(0, Mathf.Cos(a) * radius, Mathf.Sin(a) * radius);
                Vector3 nextZ = center + new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius);

                Debug.DrawLine(prevX, nextX, color, duration);
                Debug.DrawLine(prevY, nextY, color, duration);
                Debug.DrawLine(prevZ, nextZ, color, duration);

                prevX = nextX; prevY = nextY; prevZ = nextZ;
            }
        }
#endif
    }
}