using UnityEngine;
using System.Collections;

namespace NoSlimes.Utils
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
    }
}