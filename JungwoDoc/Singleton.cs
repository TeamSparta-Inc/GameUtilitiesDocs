using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JungWoo.Utilities
{
    public class Singleton<T>
    {
        #region Singleton
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = (T)Activator.CreateInstance(typeof(T));
                }
                return _instance;
            }
        }

        public static bool isInstance { get => _instance != null; }
        #endregion
    }
}