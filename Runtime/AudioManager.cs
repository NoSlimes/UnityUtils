using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds10 = new(10f);

    public static AudioManager Instance { get; private set; }

    [SerializeField] private int minPoolSize = 10;
    [SerializeField] private int growAmount = 5;

    [SerializeField] private AudioTypeMapping[] audioMixerMappings = Array.Empty<AudioTypeMapping>();

    private readonly Stack<AudioSource> freeSources = new();
    private readonly List<AudioSource> busySources = new();

    private readonly Dictionary<AudioType, AudioMixerGroup> audioMixerGroupLookup = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        GrowPool(minPoolSize);
        StartCoroutine(PoolWatcherCoroutine());
        StartCoroutine(AudioFinishedWatcherCoroutine());

        // Setup audio mixer group lookup
        foreach (AudioTypeMapping mapping in audioMixerMappings)
        {
            audioMixerGroupLookup[mapping.type] = mapping.mixerGroup;
        }
    }

    public AudioSource PlayAudioAtPoint(AudioResource resource, Vector3 position, float volume = 1f, AudioType audioType = AudioType.SFX)
    {
        if (!resource)
            return null;

        AudioSource src = GetPooledSource();
        if (!src)
            return null;

        if (audioMixerGroupLookup.TryGetValue(audioType, out AudioMixerGroup mixerGroup))
        {
            src.outputAudioMixerGroup = mixerGroup;
        }
        else
        {
            Debug.LogWarning($"[UnityUtils AudioManager] No AudioMixerGroup found for AudioType {audioType}. Using default output.");
            src.outputAudioMixerGroup = null;
        }

        src.transform.position = position;
        src.resource = resource;
        src.spatialBlend = 1f;
        src.volume = volume;
        src.loop = false;
        src.Play();

        return src;
    }

    public AudioSource PlayAudioAtPoint(AudioResource resource, Vector3 position, AudioType audioType = AudioType.SFX) => PlayAudioAtPoint(resource, position, 1f, audioType);

    public AudioSource PlayAudioAttached(AudioResource resource, Transform parent, float volume = 1f, AudioType audioType = AudioType.SFX)
    {
        if (!resource) return null;

        AudioSource src = PlayAudioAtPoint(resource, parent.position, volume, audioType);
        if (!src) return null;
        src.transform.SetParent(parent);

        return src;
    }
    public AudioSource PlayAudioAttached(AudioResource resource, Transform parent, AudioType audioType = AudioType.SFX) => PlayAudioAttached(resource, parent, 1f, audioType);

    public void StopAudioSource(AudioSource src)
    {
        ReleaseSource(src);
    }

    private AudioSource GetPooledSource()
    {
        while (freeSources.Count > 0)
        {
            AudioSource src = freeSources.Pop();

            if (!src)
                continue;

            busySources.Add(src);
            src.gameObject.hideFlags = HideFlags.None;

            src.gameObject.SetActive(true);
            return src;
        }

        GrowPool(growAmount);
        return freeSources.Count > 0 ? GetPooledSource() : null;
    }

    private void ReleaseSource(AudioSource src)
    {
        if (!src)
            return;

        int index = busySources.IndexOf(src);
        ReleaseSourceAtIndex(index);
    }

    private void ReleaseSourceAtIndex(int index)
    {
        if( index < 0 || index >= busySources.Count)
        {
            Debug.LogWarning("[UnityUtils AudioManager] Attempted to release AudioSource that is not managed by AudioManager.");
            return;
        }

        AudioSource src = busySources[index];
        if (!src)
        {
            busySources.RemoveAt(index);
            Debug.LogWarning("[UnityUtils AudioManager] Attempted to release AudioSource that has been destroyed.");
            return;
        }

        busySources.RemoveAt(index);

        src.Stop();
        src.outputAudioMixerGroup = null;
        src.resource = null;
        src.loop = false;
        src.spatialBlend = 0f;
        src.volume = 1f;

        src.transform.position = Vector3.zero;
        src.transform.SetParent(transform);

        src.gameObject.SetActive(false);
        freeSources.Push(src);
    }

    private void GrowPool(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject go = new("Pooled Audio Source");
            go.transform.SetParent(transform);
            go.hideFlags = HideFlags.HideInHierarchy;

            AudioSource src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;

            go.SetActive(false);
            freeSources.Push(src);
        }
    }

    private IEnumerator AudioFinishedWatcherCoroutine()
    {
        yield return null;

        while (true)
        {
            for (int i = busySources.Count - 1; i >= 0; i--)
            {
                AudioSource src = busySources[i];
                if (!src)
                {
                    busySources.RemoveAt(i);
                    continue;
                }
                if (!src.isPlaying)
                {
                    ReleaseSourceAtIndex(i);
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

            while (freeSources.Count > minPoolSize)
            {
                AudioSource src = freeSources.Pop();
                if (!src || busySources.Contains(src))
                    continue;

                if (src)
                    Destroy(src.gameObject);
            }
        }
    }

    public enum AudioType
    {
        Music,
        SFX,
        Voice,
        Ambient
    }

    [Serializable]
    public struct AudioTypeMapping
    {
        public AudioType type;
        public AudioMixerGroup mixerGroup;
    }
}
