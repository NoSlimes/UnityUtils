using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds10 = new(10f);
    public static ParticleManager Instance { get; private set; }

    [SerializeField] private ParticleMapping[] particleMappings;
    [SerializeField] private int defaultGrowAmount = 5;

    private readonly Dictionary<string, ParticleSystem> prefabLookup = new();
    private readonly Dictionary<ParticleSystem, string> reversePrefabLookup = new();
    private readonly Dictionary<string, Stack<ParticleSystem>> freePools = new();
    private readonly Dictionary<string, int> minPoolSizes = new();

    private readonly List<ActiveParticle> busyParticles = new();

    private struct ActiveParticle
    {
        public string key;
        public ParticleSystem system;
    }

    [Serializable]
    public struct ParticleMapping
    {
        public string key;
        public ParticleSystem prefab;
        public int prewarmCount; 
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        foreach (var mapping in particleMappings)
        {
            prefabLookup[mapping.key] = mapping.prefab;
            reversePrefabLookup[mapping.prefab] = mapping.key;
            freePools[mapping.key] = new Stack<ParticleSystem>();
            minPoolSizes[mapping.key] = mapping.prewarmCount;

            GrowSubPool(mapping.key, mapping.prewarmCount);
        }

        StartCoroutine(PoolWatcherCoroutine());
        StartCoroutine(ParticleFinishedWatcherCoroutine());
    }

    public ParticleSystem Play(string key, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!freePools.ContainsKey(key))
        {
            Debug.LogWarning($"[ParticleManager] No pool found for key: {key}");
            return null;
        }

        ParticleSystem ps = GetPooledParticle(key);
        if (!ps) return null;

        ps.transform.position = position;
        ps.transform.rotation = rotation;
        if (parent != null) ps.transform.SetParent(parent);

        ps.gameObject.SetActive(true);
        ps.Play(true);

        busyParticles.Add(new ActiveParticle { key = key, system = ps });
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
            if (busyParticles[i].system == ps)
            {
                ReleaseParticleAtIndex(i);
                return;
            }
        }
    }

    private ParticleSystem GetPooledParticle(string key)
    {
        Stack<ParticleSystem> pool = freePools[key];
        while (pool.Count > 0)
        {
            ParticleSystem ps = pool.Pop();
            if (!ps) continue;
            return ps;
        }

        GrowSubPool(key, defaultGrowAmount);
        return GetPooledParticle(key);
    }

    private void ReleaseParticleAtIndex(int index)
    {
        ActiveParticle active = busyParticles[index];
        busyParticles.RemoveAt(index);

        if (active.system == null) return;

        // Clear trails and simulated particles so they don't teleport on next use
        active.system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        active.system.transform.SetParent(transform);
        active.system.gameObject.SetActive(false);
        freePools[active.key].Push(active.system);
    }

    private void GrowSubPool(string key, int amount)
    {
        ParticleSystem prefab = prefabLookup[key];
        for (int i = 0; i < amount; i++)
        {
            ParticleSystem instance = Instantiate(prefab, transform);
            instance.gameObject.name = $"Pooled_{key}";
            instance.gameObject.SetActive(false);
            freePools[key].Push(instance);
        }
    }

    private IEnumerator ParticleFinishedWatcherCoroutine()
    {
        while (true)
        {
            for (int i = busyParticles.Count - 1; i >= 0; i--)
            {
                ParticleSystem ps = busyParticles[i].system;

                // IsAlive(true) checks all child emitters as well
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

            foreach (var poolEntry in freePools)
            {
                string key = poolEntry.Key;
                Stack<ParticleSystem> stack = poolEntry.Value;
                int minSize = minPoolSizes[key];

                // Shrink this specific sub-pool if it exceeded its prewarm count
                while (stack.Count > minSize)
                {
                    ParticleSystem ps = stack.Pop();
                    if (ps) Destroy(ps.gameObject);
                }
            }
        }
    }
}