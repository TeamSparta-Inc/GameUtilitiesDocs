using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JungWoo.Utilities
{
    public class ResourceManager : MonoSingleton<ResourceManager>
    {
        // 타입별 에셋 로드 캐시
        private Dictionary<System.Type, Dictionary<AddressableEnum, AsyncOperationHandle>> assetLoadCache
            = new Dictionary<System.Type, Dictionary<AddressableEnum, AsyncOperationHandle>>();

        // 인스턴스화된 게임오브젝트 핸들 관리
        private Dictionary<GameObject, AsyncOperationHandle<GameObject>> instantiatedAssetHandles
            = new Dictionary<GameObject, AsyncOperationHandle<GameObject>>();

        private Dictionary<AddressableEnum, AsyncOperationHandle> GetTypedCache<T>()
        {
            var type = typeof(T);
            if (!assetLoadCache.ContainsKey(type))
            {
                assetLoadCache[type] = new Dictionary<AddressableEnum, AsyncOperationHandle>();
            }
            return assetLoadCache[type];
        }

        // 에셋 로드 (비동기)
        public void LoadAsset<T>(AddressableEnum key, Action<T> onLoadComplete = null, Action<Exception> onLoadError = null)
        {
            var cache = GetTypedCache<T>();

            if (cache.TryGetValue(key, out var cachedHandle))
            {
                if (cachedHandle.Result is T cachedAsset)
                {
                    onLoadComplete?.Invoke(cachedAsset);
                    return;
                }
            }

            var loadOperation = Addressables.LoadAssetAsync<T>(key.ToString());
            loadOperation.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    cache[key] = op;
                    onLoadComplete?.Invoke(op.Result);
                }
                else
                {
                    onLoadError?.Invoke(new Exception($"Failed to load asset: {key}"));
                }
            };
        }

        // 게임오브젝트 인스턴스화
        public void InstantiateAsset(AddressableEnum key, Vector3 position, Quaternion rotation,
            Transform parent = null, Action<GameObject> onComplete = null, Action<Exception> onError = null)
        {
            var instantiateOperation = parent != null
                ? Addressables.InstantiateAsync(key.ToString(), position, rotation, parent)
                : Addressables.InstantiateAsync(key.ToString(), position, rotation);

            instantiateOperation.Completed += (op) =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    instantiatedAssetHandles[op.Result] = instantiateOperation;
                    onComplete?.Invoke(op.Result);
                }
                else
                {
                    onError?.Invoke(new Exception($"Failed to instantiate asset: {key}"));
                }
            };
        }

        // 인스턴스화된 게임오브젝트 해제
        public void ReleaseInstance(GameObject instance)
        {
            if (instantiatedAssetHandles.TryGetValue(instance, out var handle))
            {
                Addressables.ReleaseInstance(instance);
                Addressables.Release(handle);
                instantiatedAssetHandles.Remove(instance);
            }
            else
            {
                Debug.LogWarning($"Tried to release an object that wasn't instantiated by ResourceManager: {instance.name}");
            }
        }

        // 특정 타입의 특정 에셋 해제
        public void ReleaseAsset<T>(AddressableEnum key)
        {
            var cache = GetTypedCache<T>();
            if (cache.TryGetValue(key, out var handle))
            {
                Addressables.Release(handle);
                cache.Remove(key);
            }
        }

        // 모든 리소스 해제
        public void ReleaseAll()
        {
            // 로드된 에셋 해제
            foreach (var typeCache in assetLoadCache.Values)
            {
                foreach (var handle in typeCache.Values)
                {
                    Addressables.Release(handle);
                }
                typeCache.Clear();
            }
            assetLoadCache.Clear();

            // 인스턴스화된 오브젝트 해제
            foreach (var instance in new List<GameObject>(instantiatedAssetHandles.Keys))
            {
                ReleaseInstance(instance);
            }
        }

        private void OnDestroy()
        {
            ReleaseAll();
        }
    }
}