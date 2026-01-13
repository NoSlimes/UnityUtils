using System;
using System.Collections.Generic;
using UnityEngine;

namespace NoSlimes.UnityUtils.Runtime
{
    public class ObjectPool<T> where T : Component
    {
        private readonly Stack<T> free = new();
        private readonly List<T> busy = new();

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
            this.min = min;
            this.max = max;
            this.grow = grow;
            this.shrinkDelay = shrinkDelay;
            this.factory = factory;
            this.onGet = onGet;
            this.onRelease = onRelease;
            this.isAlive = isAlive;

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
            while (free.Count > 0)
            {
                var obj = free.Pop();
                if (!obj) continue;

                busy.Add(obj);
                obj.gameObject.SetActive(true);
                onGet(obj);

                if (free.Count <= min)
                    excessStartTime = -1f;

                return obj;
            }

            if (Count < max)
            {
                GrowPool(grow);
                return Get();
            }

            return null;
        }

        public void Release(T obj)
        {
            if (!obj) return;
            int index = busy.IndexOf(obj);
            if (index < 0) return;

            busy.RemoveAt(index);
            onRelease(obj);
            obj.gameObject.SetActive(false);
            free.Push(obj);

            if (free.Count > min && excessStartTime < 0f)
                excessStartTime = Time.time;
        }

        /// <summary>
        /// Call once per frame.
        /// Handles auto-return and delayed shrinking.
        /// </summary>
        public void Update()
        {
            for (int i = busy.Count - 1; i >= 0; i--)
            {
                var obj = busy[i];
                if (!obj || !isAlive(obj))
                    Release(obj);
            }

            if (free.Count > min && excessStartTime >= 0f)
            {
                if (Time.time - excessStartTime >= shrinkDelay)
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