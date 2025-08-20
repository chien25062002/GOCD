using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GOCD.Framework
{
    public class PoolPrefabItem : MonoBase
    {
        [Title("Config")]
        [SerializeField] PoolPrefabConfig _config;

        bool _subscribed;

        protected virtual void Awake()
        {
            // Nếu item cần tồn tại cross-scene thì đảm bảo nằm trong DDOL
            if (_config != null && _config.dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
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
            if (_config != null && _config.dontDestroyOnLoad && !MonoCallback.IsDestroyed && MonoCallback.Instance != null)
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
        }

        void TryUnsubscribe()
        {
            if (!_subscribed) return;
            if (!MonoCallback.IsDestroyed && MonoCallback.Instance != null)
            {
                MonoCallback.Instance.EventActiveSceneChanged -= MonoCallback_EventActiveSceneChanged;
            }
            _subscribed = false;
        }

        void MonoCallback_EventActiveSceneChanged(Scene sceneCurrent, Scene sceneNext)
        {
            // ====== GUARDS RẤT QUAN TRỌNG ======
            if (!this) return;                    // Unity fake-null on destroyed component
            if (!gameObject) return;
            if (_config == null) return;

            var go = GameObjectCached;            // có thể fake-null
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
