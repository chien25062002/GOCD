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

            public bool IsValid => reference != null
                                   && reference.TryGetTarget(out var go)
                                   && go != null;

            public GameObject GameObject
            {
                get
                {
                    if (reference != null && reference.TryGetTarget(out var go))
                        return go;
                    return null;
                }
            }
        }

        static int _frameCounter = 0;
        const int AutoCleanInterval = 3600;

        static readonly Dictionary<int, WeakGameObjectRef> _cacheMap = new();

        // ---- Lazy init flags ----
        static bool _inited;
        static bool _updateHooked;
        static bool _quitting;

        // Không làm gì “nặng” ở static ctor nữa
        static ExtensionsMonoBehaviour() { /* no side-effects */ }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void RuntimeInit()
        {
            EnsureInit();
        }

        static void EnsureInit()
        {
            if (_inited) return;
            _inited = true;

            // Các hook an toàn (không đụng MonoCallback)
            SceneManager.activeSceneChanged += (_, _) => ClearAllComponentCache();
            Application.quitting += OnAppQuitting;

            // Thử hook Update qua MonoCallback nếu đã sẵn sàng, nếu chưa thì defer
            TryHookUpdate();
            SceneManager.sceneLoaded += OnSceneLoadedTryHook; // dự phòng: khi scene mới load, thử hook lại
        }

        static void OnAppQuitting()
        {
            _quitting = true;
            ClearAllComponentCache();
            TryUnhookUpdate(); // gỡ nếu có
        }

        static void OnSceneLoadedTryHook(Scene _, LoadSceneMode __)
        {
            TryHookUpdate();
        }

        static void TryHookUpdate()
        {
            if (_updateHooked || _quitting) return;

            // MonoCallback có thể chưa spawn ở thời điểm này
            if (!MonoCallback.IsDestroyed && MonoCallback.Instance != null)
            {
                MonoCallback.Instance.EventUpdate += AutoCleanCache;
                _updateHooked = true;
            }
            // nếu chưa có Instance, cứ để SceneLoaded gọi lại TryHookUpdate lần sau
        }

        static void TryUnhookUpdate()
        {
            if (!_updateHooked) return;
            if (!MonoCallback.IsDestroyed && MonoCallback.Instance != null)
            {
                MonoCallback.Instance.EventUpdate -= AutoCleanCache;
            }
            _updateHooked = false;
        }

        /// <summary>
        /// Lấy component có cache. Không cache null component.
        /// </summary>
        public static T GetCachedComponent<T>(this GameObject go) where T : Component
        {
            EnsureInit();

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
                return (T)comp;

            var go = entry.GameObject;
            if (go == null) return null;

            comp = go.GetComponent<T>();
            if (comp != null)
                entry.components[type] = comp;

            return (T)comp;
        }

        /// <summary>Xoá toàn bộ cache thủ công.</summary>
        public static void ClearAllComponentCache()
        {
            EnsureInit();
            _cacheMap.Clear();
        }

        /// <summary>Xoá cache của 1 GameObject.</summary>
        public static void ClearComponentCache(this GameObject go)
        {
            EnsureInit();
            if (go == null) return;
            int id = go.GetInstanceID();
            _cacheMap.Remove(id);
        }

        static void AutoCleanCache()
        {
            // Có thể bị gọi sau khi quitting — bảo vệ nhẹ
            if (_quitting) return;

            _frameCounter++;
            if (_frameCounter < AutoCleanInterval) return;
            _frameCounter = 0;

            var keysToRemove = new List<int>();
            foreach (var kvp in _cacheMap)
            {
                if (!kvp.Value.IsValid)
                    keysToRemove.Add(kvp.Key);
            }

            for (int i = 0; i < keysToRemove.Count; i++)
                _cacheMap.Remove(keysToRemove[i]);
        }
    }
}
