using UnityEngine;

namespace NoSlimes.UnityUtils.Common
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

        public static float NextFloat(this System.Random rng)
            => (float)rng.NextDouble();

        public static float NextFloat(this System.Random rng, float min, float max)
            => (float)(rng.NextDouble() * (max - min) + min);
    }
}
