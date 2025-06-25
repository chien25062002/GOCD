using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GOCD.Framework
{
    public static class ExtensionsMonoBehaviour
    {
        class WeakGameObjectRef
        {
            public WeakReference<GameObject> reference;
            public Dictionary<Type, Component> components;

            public bool IsValid => reference.TryGetTarget(out var go) && go != null;

            public GameObject GameObject
            {
                get
                {
                    reference.TryGetTarget(out var go);
                    return go;
                }
            }
        }
        
        static int _frameCounter = 0;
        const int AutoCleanInterval = 3600; // Clean mỗi 3600 frame (nếu game 60fps thì là sau 60s clean 1 lần)

        static readonly Dictionary<int, WeakGameObjectRef> _cacheMap = new();

        static ExtensionsMonoBehaviour()
        {
            // Clear cache khi đổi scene hoặc thoát game
            SceneManager.activeSceneChanged += (_, _) => ClearAllComponentCache();
            Application.quitting += ClearAllComponentCache;
            
            // Hook vào Unity update
            MonoCallback.Instance.EventUpdate += AutoCleanCache;
        }

        /// <summary>
        /// Lấy component từ GameObject có cache. Không cache null component.
        /// </summary>
        public static T GetCachedComponent<T>(this GameObject go) where T : Component
        {
            if (go == null) return null;

            int id = go.GetInstanceID();

            if (_cacheMap.TryGetValue(id, out var entry))
            {
                if (!entry.IsValid)
                {
                    _cacheMap.Remove(id);
                }
                else
                {
                    return GetOrAdd<T>(entry);
                }
            }

            // Nếu chưa có hoặc đã mất valid → tạo mới
            var newRef = new WeakGameObjectRef
            {
                reference = new WeakReference<GameObject>(go),
                components = new Dictionary<Type, Component>()
            };
            _cacheMap[id] = newRef;
            return GetOrAdd<T>(newRef);
        }

        static T GetOrAdd<T>(WeakGameObjectRef entry) where T : Component
        {
            var type = typeof(T);
            if (entry.components.TryGetValue(type, out var comp))
            {
                return (T)comp;
            }

            var go = entry.GameObject;
            if (go == null) return null;

            comp = go.GetComponent<T>();
            if (comp != null)
            {
                entry.components[type] = comp; // ⚠️ Chỉ cache nếu không null
            }

            return (T)comp;
        }

        /// <summary>
        /// Xoá toàn bộ cache thủ công nếu muốn.
        /// </summary>
        public static void ClearAllComponentCache()
        {
            _cacheMap.Clear();
        }

        /// <summary>
        /// Xoá cache của một GameObject cụ thể (ví dụ khi return về pool).
        /// </summary>
        public static void ClearComponentCache(this GameObject go)
        {
            if (go == null) return;
            int id = go.GetInstanceID();
            _cacheMap.Remove(id);
        }
        
        static void AutoCleanCache()
        {
            _frameCounter++;
            if (_frameCounter < AutoCleanInterval) return;
            _frameCounter = 0;

            var keysToRemove = new List<int>();
            foreach (var kvp in _cacheMap)
            {
                if (!kvp.Value.IsValid)
                    keysToRemove.Add(kvp.Key);
            }

            foreach (var key in keysToRemove)
                _cacheMap.Remove(key);
        }
    }
}
