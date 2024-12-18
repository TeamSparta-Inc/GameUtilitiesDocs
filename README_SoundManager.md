# 📄 **README: SoundManager**

---

### 📌 **개요**

`SoundManager`는 Unity에서 **오디오 클립을 관리하고 재생**하는 유틸리티 클래스입니다. 이 스크립트는 Addressable 시스템을 사용하여 오디오 클립을 비동기적으로 로드하고, 오디오 소스 풀을 통해 여러 사운드를 동시에 재생할 수 있습니다.

---

### 🚀 **기능**

1. **비동기 오디오 클립 로드**  
   Addressable 시스템을 사용해 오디오 클립을 비동기적으로 로드하고 캐싱합니다.

2. **오디오 소스 풀링**  
   여러 오디오 소스를 미리 생성하여 동시에 여러 사운드를 재생할 수 있습니다.

3. **풀 자동 확장**  
   모든 오디오 소스가 사용 중일 때 자동으로 풀을 확장합니다.

4. **오디오 클립 캐싱**  
   이미 로드된 오디오 클립은 캐시에 저장되어 재사용됩니다.

---

### 🛠️ **사용법**

#### **1. 초기화**

`SoundManager`는 싱글톤으로 설계되었으며, 초기화 시 오디오 소스를 미리 생성합니다.

```csharp
private void Awake()
{
    InitializeAudioSources();
}
```

#### **2. 오디오 클립 로드**

Addressable 키를 기반으로 오디오 클립을 비동기적으로 로드합니다.

```csharp
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
            onComplete?.Invoke(op.Result);
        };
        return;
    }

    var handle = Addressables.LoadAssetAsync<AudioClip>(key);
    loadingOperations[key] = handle;

    handle.Completed += (op) =>
    {
        if (op.Status == AsyncOperationStatus.Succeeded)
        {
            audioClipCache[key] = op.Result;
            onComplete?.Invoke(op.Result);
        }
        else
        {
            onError?.Invoke(op.OperationException);
        }
        loadingOperations.Remove(key);
    };
}
```

#### **3. 오디오 재생**

로드된 오디오 클립을 재생합니다.

```csharp
public void PlaySound(string key)
{
    LoadAudioClip(key, (clip) =>
    {
        var source = GetAvailableAudioSource();
        source.clip = clip;
        source.Play();
    });
}
```

---

### 📦 **스크립트 예제**

전체 `SoundManager` 스크립트 예제:

```csharp
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
        private Dictionary<string, AudioClip> audioClipCache = new Dictionary<string, AudioClip>();
        private Dictionary<string, AsyncOperationHandle<AudioClip>> loadingOperations = new Dictionary<string, AsyncOperationHandle<AudioClip>>();

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

            var newSource = gameObject.AddComponent<AudioSource>();
            audioSourcePool.Add(newSource);
            return newSource;
        }

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
                    onComplete?.Invoke(op.Result);
                };
                return;
            }

            var handle = Addressables.LoadAssetAsync<AudioClip>(key);
            loadingOperations[key] = handle;

            handle.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    audioClipCache[key] = op.Result;
                    onComplete?.Invoke(op.Result);
                }
                else
                {
                    onError?.Invoke(op.OperationException);
                }
                loadingOperations.Remove(key);
            };
        }

        public void PlaySound(string key)
        {
            LoadAudioClip(key, (clip) =>
            {
                var source = GetAvailableAudioSource();
                source.clip = clip;
                source.Play();
            });
        }
    }
}
```

---

### ⚠️ **주의사항**

1. **Addressable 키**  
   오디오 클립은 Addressable 시스템에 등록된 키를 사용해야 합니다.

2. **풀 사이즈 조정**  
   `poolSize`를 조정하여 오디오 소스 풀의 초기 크기를 설정할 수 있습니다.

3. **메모리 관리**  
   필요하지 않은 오디오 클립은 캐시에서 제거하여 메모리를 관리하세요.

---

### 📚 **확장하기**

- **오디오 옵션 추가**:  
  재생 볼륨, 반복 재생 등의 옵션을 추가할 수 있습니다.

- **사운드 카테고리**:  
  효과음, 배경음악 등을 구분하여 관리할 수 있습니다.

---

이 스크립트를 활용하여 게임 내 다양한 사운드를 효율적으로 관리하세요! 🎵😊