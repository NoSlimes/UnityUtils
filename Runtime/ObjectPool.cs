using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoSlimes.UnityUtils.Runtime
{
    public class ObjectPool<T> where T : Component
    {
        private readonly Stack<T> free = new();
        private readonly HashSet<T> busy = new();

        private readonly int min;
        private readonly int max;
        private readonly int grow;
        private readonly float shrinkDelay;

        private readonly Func<T> factory;
        private readonly Action<T> onGet;
        private readonly Action<T> onRelease;
        private readonly Func<T, bool> isAlive;

        private float excessStartTime = -1f;

        public ObjectPool(
            int min,
            int max,
            int grow,
            float shrinkDelay,
            Func<T> factory,
            Action<T> onGet,
            Action<T> onRelease,
            Func<T, bool> isAlive)
        {
            if (min < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(min));
            }

            if (max < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(max));
            }

            if (grow <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(grow));
            }

            if (shrinkDelay < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(shrinkDelay));
            }

            // Clamp min to max to avoid over-allocating in Prewarm
            if (min > max)
            {
                min = max;
            }

            this.min = min;
            this.max = max;
            this.grow = grow;
            this.shrinkDelay = shrinkDelay;
            this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            this.onGet = onGet;
            this.onRelease = onRelease;
            this.isAlive = isAlive ?? throw new ArgumentNullException(nameof(isAlive));

            Prewarm(min);
        }

        private int Count => free.Count + busy.Count;

        private void Prewarm(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                var obj = factory();
                obj.gameObject.SetActive(false);
                free.Push(obj);
            }
        }

        public T Get()
        {
            while (true)
            {
                while (free.Count > 0)
                {
                    var obj = free.Pop();
                    if (!obj) continue;

                    busy.Add(obj);
                    obj.gameObject.SetActive(true);
                    onGet?.Invoke(obj);

                    if (free.Count <= min)
                        excessStartTime = -1f;

                    return obj;
                }

                if (Count >= max)
                    return null;

                GrowPool(grow);
            }
        }

        public void Release(T obj)
        {
            if (!obj) return;
            if (!busy.Contains(obj)) return;

            busy.Remove(obj);
            onRelease?.Invoke(obj);
            obj.gameObject.SetActive(false);
            free.Push(obj);

            if (free.Count > min && excessStartTime < 0f)
                excessStartTime = Time.unscaledTime;
        }

        /// <summary>
        /// Call once per frame.
        /// Handles auto-return and delayed shrinking.
        /// </summary>
        public void Update()
        {
            // Snapshot to avoid modifying busy while iterating (HashSet)
            if (busy.Count > 0)
            {
                var snapshot = new List<T>(busy);
                foreach (var obj in snapshot)
                {
                    if (!obj || !isAlive(obj))
                        Release(obj);
                }
            }

            if (free.Count > min && excessStartTime >= 0f)
            {
                if (Time.unscaledTime - excessStartTime >= shrinkDelay)
                {
                    while (free.Count > min)
                    {
                        var obj = free.Pop();
                        if (obj)
                            UnityEngine.Object.Destroy(obj.gameObject);
                    }

                    excessStartTime = -1f;
                }
            }
        }

        private void GrowPool(int amount)
        {
            int canCreate = Mathf.Min(amount, max - Count);
            for (int i = 0; i < canCreate; i++)
            {
                var obj = factory();
                obj.gameObject.SetActive(false);
                free.Push(obj);
            }
        }
    }
}