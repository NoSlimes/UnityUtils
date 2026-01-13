using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace NoSlimes.UnityUtils.Runtime
{
    public class AudioManager : MonoBehaviour
    {
        private static readonly WaitForSeconds WatchInterval = new(0.05f); 

        public static AudioManager Instance { get; private set; }

        [SerializeField] private int minPoolSize = 10;
        [SerializeField] private int maxPoolSize = 50;
        [SerializeField] private int growAmount = 5;
        [SerializeField] private float shrinkDelay = 10f;

        [SerializeField] private AudioTypeMapping[] audioMixerMappings = Array.Empty<AudioTypeMapping>();

        private ObjectPool<AudioSource> pool;
        private readonly System.Collections.Generic.Dictionary<AudioType, AudioMixerGroup> audioMixerGroupLookup = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            foreach (var mapping in audioMixerMappings)
                audioMixerGroupLookup[mapping.type] = mapping.mixerGroup;

            pool = new ObjectPool<AudioSource>(
                minPoolSize,
                maxPoolSize,
                growAmount,
                shrinkDelay,
                CreateSource,
                OnGetSource,
                OnReleaseSource,
                src => src.isPlaying
            );

            StartCoroutine(PoolWatcher());
        }

        private IEnumerator PoolWatcher()
        {
            yield return null;

            while (true)
            {
                pool.Update();
                yield return WatchInterval;
            }
        }

        private AudioSource CreateSource()
        {
            GameObject go = new("Pooled Audio Source");
            go.transform.SetParent(transform);
            go.hideFlags = HideFlags.HideInHierarchy;

            AudioSource src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            go.SetActive(false);
            return src;
        }

        private void OnGetSource(AudioSource src)
        {
            src.gameObject.hideFlags = HideFlags.None;
            src.transform.SetParent(null);
        }

        private void OnReleaseSource(AudioSource src)
        {
            src.Stop();
            src.outputAudioMixerGroup = null;
            src.resource = null;
            src.loop = false;
            src.spatialBlend = 0f;
            src.volume = 1f;

            src.transform.position = Vector3.zero;
            src.transform.SetParent(transform);
        }

        public AudioSource PlayAudioAtPoint(AudioResource resource, Vector3 position, float volume = 1f, AudioType audioType = AudioType.SFX)
        {
            if (!resource)
                return null;

            AudioSource src = pool.Get();
            if (!src)
                return null;

            if (audioMixerGroupLookup.TryGetValue(audioType, out AudioMixerGroup mixerGroup))
                src.outputAudioMixerGroup = mixerGroup;
            else
                src.outputAudioMixerGroup = null;

            src.transform.position = position;
            src.resource = resource;
            src.spatialBlend = 1f;
            src.volume = volume;
            src.loop = false;
            src.Play();

            return src;
        }

        public AudioSource PlayAudioAtPoint(AudioResource resource, Vector3 position, AudioType audioType = AudioType.SFX)
            => PlayAudioAtPoint(resource, position, 1f, audioType);

        public AudioSource PlayAudioAttached(AudioResource resource, Transform parent, float volume = 1f, AudioType audioType = AudioType.SFX)
        {
            if (!resource) return null;

            AudioSource src = PlayAudioAtPoint(resource, parent.position, volume, audioType);
            if (!src) return null;

            src.transform.SetParent(parent);
            return src;
        }

        public AudioSource PlayAudioAttached(AudioResource resource, Transform parent, AudioType audioType = AudioType.SFX)
            => PlayAudioAttached(resource, parent, 1f, audioType);

        public void StopAudioSource(AudioSource src)
        {
            pool.Release(src);
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
}
