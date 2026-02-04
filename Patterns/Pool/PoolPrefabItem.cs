using CodeSketch.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

using CodeSketch.Mono;

namespace CodeSketch.Patterns.Pool
{
    public class PoolPrefabItem : MonoBaseOpt
    {
        [Header("Config")]
        [SerializeField] PoolPrefabConfig _config;

        public PoolPrefabConfig Config
        {
            get => _config;
            set => _config = value;
        }
        
        protected virtual void Awake()
        {
            if (_config != null)
            {
                Pooler.Attach(this);
            }

            GetCachedInterfaces<IPoolRelease>();
            GetCachedInterfaces<IPoolReleaseAync>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            TrySubscribe();
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            TryUnsubscribe();
        }
        
        protected virtual void OnDestroy()
        {
            TryUnsubscribe();
        }

        protected override void Start()
        {
            base.Start();
            TrySubscribe(); // phòng trường hợp OnEnable chưa kịp
        }

        void TrySubscribe()
        {
            MonoCallback.SafeInstance.EventActiveSceneChanged += MonoCallback_EventActiveSceneChanged;
        }

        void TryUnsubscribe()
        {
            MonoCallback.SafeInstance.EventActiveSceneChanged -= MonoCallback_EventActiveSceneChanged;
        }

        void MonoCallback_EventActiveSceneChanged(Scene sceneCurrent, Scene sceneNext)
        {
            // ====== GUARDS RẤT QUAN TRỌNG ======
            if (!this || !gameObject || _config == null || !GameObjectCached) return; // Unity fake-null on destroyed component

            // Nếu đã release vào pool rồi -> bỏ qua
            if (PoolPrefabGlobal.IsReleased(this)) return;

            // Nếu không có mapping prefab/pool -> đừng tự Destroy ở đây,
            // vì scene change sẽ xử lý; tránh double-destroy trong callback.
            if (_config.Prefab == null)
                return;

            // Trả về pool (an toàn idempotent vì Global có _releasedInstances)
            PoolPrefabGlobal.Release(_config, this);
        }
    }
}