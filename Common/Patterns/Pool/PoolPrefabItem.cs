using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GOCD.Framework
{
    public class PoolPrefabItem : MonoBase
    {
        [Title("Config")]
        [SerializeField] PoolPrefabConfig _config;

        protected virtual void  OnDestroy()
        {
            gameObject.ClearComponentCache();
        }

        protected virtual void Start()
        {
            if (_config.dontDestroyOnLoad && !MonoCallback.IsDestroyed)
            {
                MonoCallback.Instance.EventActiveSceneChanged += MonoCallback_EventActiveSceneChanged;
            }
        }

        void MonoCallback_EventActiveSceneChanged(Scene arg1, Scene arg2)
        {
            if (GameObjectCached != null && GameObjectCached.activeInHierarchy)
                PoolPrefabGlobal.Release(_config, GameObjectCached);
        }
    }
}
