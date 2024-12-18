using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JungWoo.Utilities
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        #region Singleton
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<T>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject(typeof(T).Name);
                        _instance = go.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        public static bool isInstance { get => _instance != null; }
        #endregion

        [SerializeField] protected bool isDontDestroy;

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning($"Another instance of {typeof(T).Name} already exists. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            _instance = (T)this;

            if (isDontDestroy)
            {
                transform.parent = null; // 부모 객체가 있다면 분리
                DontDestroyOnLoad(gameObject);
            }

            OnAwake();
        }

        protected virtual void OnAwake()
        {
            // 자식 클래스에서 오버라이드하여 Awake 로직 구현
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}