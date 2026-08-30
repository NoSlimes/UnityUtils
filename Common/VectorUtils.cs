using UnityEngine;
using System.Collections;

namespace NoSlimes.UnityUtils.Common
{
    public static class VectorUtils
    {
        public static Vector3 WithX(this Vector3 vector, float newX)
        {
            return new Vector3(newX, vector.y, vector.z);
        }

        public static Vector3 WithY(this Vector3 vector, float newY)
        {
            return new Vector3(vector.x, newY, vector.z);
        }

        public static Vector3 WithZ(this Vector3 vector, float newZ)
        {
            return new Vector3(vector.x, vector.y, newZ);
        }

        public static Vector2 WithX(this Vector2 vector, float newX)
        {
            return new Vector2(newX, vector.y);
        }

        public static Vector2 WithY(this Vector2 vector, float newY)
        {
            return new Vector2(vector.x, newY);
        }

        public static Vector4 WithX(this Vector4 vector, float newX)
        {
            return new Vector4(newX, vector.y, vector.z, vector.w);
        }

        public static Vector4 WithY(this Vector4 vector, float newY)
        {
            return new Vector4(vector.x, newY, vector.z, vector.w);
        }

        public static Vector4 WithZ(this Vector4 vector, float newZ)
        {
            return new Vector4(vector.x, vector.y, newZ, vector.w);
        }

        public static Vector4 WithW(this Vector4 vector, float newW)
        {
            return new Vector4(vector.x, vector.y, vector.z, newW);
        }
    }
}