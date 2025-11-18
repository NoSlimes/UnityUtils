using UnityEngine;

namespace NoSlimes.Utils.Common
{
    public static class RandomUtils 
    {
        public static Vector3 RandomVec3(float minValue, float maxValue)
        {
            return new Vector3(
                Random.Range(minValue, maxValue),
                Random.Range(minValue, maxValue),
                Random.Range(minValue, maxValue)
            );
        }


    }
}
