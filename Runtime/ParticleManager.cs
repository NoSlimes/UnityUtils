using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoSlimes.UnityUtils.Runtime
{
    public class ParticleManager : MonoBehaviour
    {
        private static readonly WaitForSeconds WatchInterval = new(0.05f);

        public static ParticleManager Instance { get; private set; }

        [Serializable]
        public struct ParticleMapping
        {
            public string Key;
            public ParticleSystem Prefab;
            public int Prewarm;
            public int Max;
        }

        [SerializeField] private ParticleMapping[] particleMappings;
        [SerializeField] private int growAmount = 5;
        [SerializeField] private float shrinkDelay = 10f;

        private readonly Dictionary<string, ObjectPool<ParticleSystem>> pools = new();
        private readonly Dictionary<string, ParticleSystem> prefabLookup = new();
        private readonly Dictionary<string, string> reversePrefabLookup = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            foreach (var m in particleMappings)
            {
                prefabLookup[m.Key] = m.Prefab;
                reversePrefabLookup[m.Prefab.name] = m.Key;

                pools[m.Key] = new ObjectPool<ParticleSystem>(
                    min: m.Prewarm,
                    max: m.Max,
                    grow: growAmount,
                    shrinkDelay: shrinkDelay,
                    factory: () =>
                    {
                        var ps = Instantiate(m.Prefab, transform);
                        ps.gameObject.name = $"Pooled_{m.Key}";
                        return ps;
                    },
                    onGet: ps => ps.Play(true),
                    onRelease: ps =>
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                        ps.transform.SetParent(transform);
                    },
                    isAlive: ps => ps.IsAlive(true)
                );
            }

            StartCoroutine(PoolWatcherCoroutine());
        }

        private IEnumerator PoolWatcherCoroutine()
        {
            yield return null;

            while (true)
            {
                foreach (var pool in pools.Values)
                    pool.Update();

                yield return WatchInterval;
            }
        }

        public ParticleSystem Play(string key, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!pools.TryGetValue(key, out var pool))
            {
                Debug.LogWarning($"[ParticleManager] No pool found for key {key}");
                return null;
            }

            var ps = pool.Get();
            if (!ps) return null;

            ps.transform.SetPositionAndRotation(position, rotation);
            if (parent) ps.transform.SetParent(parent);
            return ps;
        }

        public ParticleSystem Play(ParticleSystem prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!prefab || !reversePrefabLookup.TryGetValue(prefab.name, out string key))
            {
                Debug.LogWarning($"[ParticleManager] Prefab {(prefab != null ? prefab.name : "null")} is not registered.");
                return null;
            }

            return Play(key, position, rotation, parent);
        }

        public void Stop(ParticleSystem ps)
        {
            if (!ps) return;

            foreach (var pool in pools.Values)
            {
                pool.Release(ps);
            }
        }
    }
}
