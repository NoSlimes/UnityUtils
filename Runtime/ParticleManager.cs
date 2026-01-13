using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    private class PoolInfo
    {
        public int MinSize;
        public int MaxSize;
        public Stack<ParticleSystem> FreeStack;

        public PoolInfo(int minSize, int maxSize)
        {
            MinSize = minSize;
            MaxSize = maxSize;
            FreeStack = new Stack<ParticleSystem>();
        }
    }

    private static readonly WaitForSeconds _waitForSeconds10 = new(10f);
    public static ParticleManager Instance { get; private set; }

    [SerializeField] private ParticleMapping[] particleMappings;
    [SerializeField] private int defaultGrowAmount = 5;

    private readonly Dictionary<string, ParticleSystem> prefabLookup = new();
    private readonly Dictionary<ParticleSystem, string> reversePrefabLookup = new();
    private readonly Dictionary<string, PoolInfo> pools = new();
    private readonly List<ActiveParticle> busyParticles = new();

    private struct ActiveParticle
    {
        public string Key;
        public ParticleSystem System;
    }

    [Serializable]
    public struct ParticleMapping
    {
        public string Key;
        public ParticleSystem Prefab;
        public int PrewarmCount;
        public int MaxPoolSize;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (ParticleMapping mapping in particleMappings)
        {
            prefabLookup[mapping.Key] = mapping.Prefab;
            reversePrefabLookup[mapping.Prefab] = mapping.Key;

            PoolInfo poolInfo = new(mapping.PrewarmCount, mapping.MaxPoolSize);
            pools[mapping.Key] = poolInfo;

            GrowSubPool(mapping.Key, mapping.PrewarmCount);
        }

        StartCoroutine(PoolWatcherCoroutine());
        StartCoroutine(ParticleFinishedWatcherCoroutine());
    }

    public ParticleSystem Play(string key, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!pools.ContainsKey(key))
        {
            Debug.LogWarning($"[ParticleManager] No pool found for key: {key}");
            return null;
        }

        ParticleSystem ps = GetPooledParticle(key);
        if (!ps) return null;

        ps.transform.SetPositionAndRotation(position, rotation);
        if (parent != null) ps.transform.SetParent(parent);

        ps.gameObject.SetActive(true);
        ps.Play(true);

        busyParticles.Add(new ActiveParticle { Key = key, System = ps });
        return ps;
    }

    public ParticleSystem Play(ParticleSystem prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (prefab == null || !reversePrefabLookup.TryGetValue(prefab, out string key))
        {
            Debug.LogWarning($"[ParticleManager] Prefab {prefab?.name} is not registered in the manager.");
            return null;
        }
        return Play(key, position, rotation, parent);
    }

    public void StopParticle(ParticleSystem ps)
    {
        if (ps == null) return;

        for (int i = busyParticles.Count - 1; i >= 0; i--)
        {
            if (busyParticles[i].System == ps)
            {
                ReleaseParticleAtIndex(i);
                return;
            }
        }
    }

    private ParticleSystem GetPooledParticle(string key)
    {
        PoolInfo poolInfo = pools[key];
        Stack<ParticleSystem> pool = poolInfo.FreeStack;

        while (pool.Count > 0)
        {
            ParticleSystem ps = pool.Pop();
            if (!ps) continue;
            return ps;
        }

        if (busyParticles.Count + pool.Count < poolInfo.MaxSize)
        {
            GrowSubPool(key, defaultGrowAmount);
            return GetPooledParticle(key);
        }

        Debug.LogWarning($"[ParticleManager] Pool for key {key} has reached its maximum size of {poolInfo.MaxSize}.");
        return null;
    }

    private void ReleaseParticleAtIndex(int index)
    {
        ActiveParticle active = busyParticles[index];
        busyParticles.RemoveAt(index);

        if (active.System == null) return;

        active.System.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        active.System.transform.SetParent(transform);
        active.System.gameObject.SetActive(false);

        pools[active.Key].FreeStack.Push(active.System);
    }

    private void GrowSubPool(string key, int amount)
    {
        PoolInfo poolInfo = pools[key];
        ParticleSystem prefab = prefabLookup[key];

        for (int i = 0; i < amount; i++)
        {
            ParticleSystem instance = Instantiate(prefab, transform);
            instance.gameObject.name = $"Pooled_{key}";
            instance.gameObject.SetActive(false);
            poolInfo.FreeStack.Push(instance);
        }
    }

    private IEnumerator ParticleFinishedWatcherCoroutine()
    {
        while (true)
        {
            for (int i = busyParticles.Count - 1; i >= 0; i--)
            {
                ParticleSystem ps = busyParticles[i].System;

                if (!ps || !ps.IsAlive(true))
                {
                    ReleaseParticleAtIndex(i);
                }
            }
            yield return null;
        }
    }

    private IEnumerator PoolWatcherCoroutine()
    {
        while (true)
        {
            yield return _waitForSeconds10;

            foreach (KeyValuePair<string, PoolInfo> poolEntry in pools)
            {
                string key = poolEntry.Key;
                PoolInfo poolInfo = poolEntry.Value;
                Stack<ParticleSystem> stack = poolInfo.FreeStack;

                while (stack.Count > poolInfo.MinSize)
                {
                    ParticleSystem ps = stack.Pop();
                    if (ps) Destroy(ps.gameObject);
                }
            }
        }
    }
}
