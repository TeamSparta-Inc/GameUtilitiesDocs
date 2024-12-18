using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using JungWoo.Utilities;

namespace JungWoo.Audio
{
    public class SoundManager : MonoSingleton<SoundManager>
    {
        // 캐시된 사운드 에셋
        private Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();
        private Dictionary<string, AsyncOperationHandle<AudioClip>> loadingOperations = new Dictionary<string, AsyncOperationHandle<AudioClip>>();

        // 여러 사운드를 동시에 재생하기 위한 오디오 소스 풀
        private List<AudioSource> audioSourcePool = new List<AudioSource>();
        [SerializeField] private int poolSize = 10;

        private void Awake()
        {
            InitializeAudioSources();
        }

        private void InitializeAudioSources()
        {
            for (int i = 0; i < poolSize; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                audioSourcePool.Add(source);
            }
        }

        private AudioSource GetAvailableAudioSource()
        {
            foreach (var source in audioSourcePool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // 사용 가능한 소스가 없는 경우 풀을 확장합니다.
            var newSource = gameObject.AddComponent<AudioSource>();
            audioSourcePool.Add(newSource);
            return newSource;
        }

        // 오디오 클립 비동기 로드 및 캐시
        public void LoadAudioClip(string key, Action<AudioClip> onComplete = null, Action<Exception> onError = null)
        {
            if (audioClipCache.TryGetValue(key, out var cachedClip))
            {
                onComplete?.Invoke(cachedClip);
                return;
            }

            if (loadingOperations.ContainsKey(key))
            {
                loadingOperations[key].Completed += (op) =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        onComplete?.Invoke(op.Result);
                    }
                    else
                    {
                        onError?.Invoke(new Exception($"Failed to load audio clip: {key}"));
                    }
                };
                return;
            }

            var handle = Addressables.LoadAssetAsync<AudioClip>(key);
            loadingOperations[key] = handle;

            handle.Completed += (op) =>
            {
                loadingOperations.Remove(key);
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    audioClipCache[key] = op.Result;
                    onComplete?.Invoke(op.Result);
                }
                else
                {
                    onError?.Invoke(new Exception($"Failed to load audio clip: {key}"));
                }
            };
        }

        // 키로 사운드 재생
        public void PlaySound(string key, bool loop = false, float volume = 1.0f)
        {
            LoadAudioClip(key, (clip) =>
            {
                AudioSource source = GetAvailableAudioSource();
                source.clip = clip;
                source.loop = loop;
                source.volume = volume;
                source.Play();
            }, (error) =>
            {
                Debug.LogError(error.Message);
            });
        }

        // 캐시에서 사용하지 않는 오디오 클립 지우기
        public void ClearUnusedClips()
        {
            List<string> keysToRemove = new List<string>();

            foreach (var pair in audioClipCache)
            {
                if (!IsClipInUse(pair.Value))
                {
                    Addressables.Release(pair.Value);
                    keysToRemove.Add(pair.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                audioClipCache.Remove(key);
            }
        }

        private bool IsClipInUse(AudioClip clip)
        {
            foreach (var source in audioSourcePool)
            {
                if (source.isPlaying && source.clip == clip)
                {
                    return true;
                }
            }
            return false;
        }

        // 캐시된 모든 클립을 지우고 진행 중인 로딩 작업 취소하기
        public void ClearAll()
        {
            ClearUnusedClips();

            foreach (var handle in loadingOperations.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }

            loadingOperations.Clear();
        }

        // SoundManager가 파괴되면 진행 중인 모든 로딩 작업 취소
        private void OnDestroy()
        {
            foreach (var handle in loadingOperations.Values)
            {
                if (!handle.IsDone)
                {
                    Addressables.Release(handle);
                }
            }

            loadingOperations.Clear();
            ClearAll();
        }
    }
}