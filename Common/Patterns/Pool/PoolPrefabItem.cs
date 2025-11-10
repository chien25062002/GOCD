using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GOCD.Framework
{
    public class PoolPrefabItem : MonoBase
    {
        [Title("Config")] [SerializeField] PoolPrefabConfig _config;

        public PoolPrefabConfig Config
        {
            get => _config;
            set => _config = value;
        }
        
        bool _subscribed;

        // ===== Cache sẵn IPoolPreRelease để không gọi GetComponents mỗi lần =====
        IPoolPreRelease[] _cachedHandlers;
        bool _cachedHandlersReady;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IPoolPreRelease[] GetCachedHandlers() => _cachedHandlers;

        protected virtual void Awake()
        {
            if (_config != null && _config.dontDestroyOnLoad)
            {
                if (transform.parent != null)
                    transform.SetParent(null, false);
                DontDestroyOnLoad(gameObject);
            }

            // Cache IPoolPreRelease ngay khi object được tạo
            if (!_cachedHandlersReady)
            {
                _cachedHandlers = GetComponentsInChildren<IPoolPreRelease>(true);
                _cachedHandlersReady = true;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            TrySubscribe();
        }

        protected virtual void Start()
        {
            TrySubscribe(); // phòng trường hợp OnEnable chưa kịp
        }

        void TrySubscribe()
        {
            if (_subscribed) return;
            if (_config != null && _config.dontDestroyOnLoad && !MonoCallback.IsDestroyed &&
                MonoCallback.Instance != null)
            {
                MonoCallback.Instance.EventActiveSceneChanged += MonoCallback_EventActiveSceneChanged;
                _subscribed = true;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            TryUnsubscribe();
        }

        protected virtual void OnDestroy()
        {
            TryUnsubscribe();
            gameObject.ClearComponentCache();

            // clear cache nhỏ (không cần thiết nhưng an toàn)
            _cachedHandlers = null;
            _cachedHandlersReady = false;
        }

        void TryUnsubscribe()
        {
            if (!_subscribed) return;
            if (!MonoCallback.IsDestroyed && MonoCallback.hasInstance)
            {
                MonoCallback.Instance.EventActiveSceneChanged -= MonoCallback_EventActiveSceneChanged;
            }

            _subscribed = false;
        }

        void MonoCallback_EventActiveSceneChanged(Scene sceneCurrent, Scene sceneNext)
        {
            // ====== GUARDS RẤT QUAN TRỌNG ======
            if (!this) return; // Unity fake-null on destroyed component
            if (!gameObject) return;
            if (_config == null) return;

            var go = GameObjectCached; // có thể fake-null
            if (!go) return;

            // Nếu đã release vào pool rồi -> bỏ qua
            if (PoolPrefabGlobal.IsReleased(go)) return;

            // Nếu không có mapping prefab/pool -> đừng tự Destroy ở đây,
            // vì scene change sẽ xử lý; tránh double-destroy trong callback.
            if (_config.prefab == null)
                return;

            // Trả về pool (an toàn idempotent vì Global có _releasedInstances)
            PoolPrefabGlobal.Release(_config, go);
        }
    }
}