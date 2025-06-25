using System.Collections.Generic;
using GOCD.Framework.Diagnostics;
using UnityEngine;
using UnityEngine.Pool;

namespace GOCD.Framework
{
    public class PoolPrefab : ObjectPool<GameObject>
    {
        PoolPrefabConfig _config;
        
        public PoolPrefabConfig Config => _config;
        
        public PoolPrefab(GameObject prefab) : base(
            () => Object.Instantiate(prefab),
            (obj) => { obj.SetActive(true); },
            (obj) => { obj.SetActive(false); },
            (obj) => { },
#if UNITY_EDITOR
            true) // Keep heavy check on editor
#else
            false)
#endif
        {
            // do nothing, just use base class constructor
        }
        
        public PoolPrefab(PoolPrefabConfig config) : base(
            () =>
            {
                GameObject createdObject = Object.Instantiate(config.prefab);

                if (config.dontDestroyOnLoad)
                    Object.DontDestroyOnLoad(createdObject);

                return createdObject;
            },
            (obj) => { obj.SetActive(true); },
            (obj) => { obj.SetActive(false); },
            (obj) => { if (obj != null) GOCDDebug.Log<PoolPrefab>($"Destroy {obj.name}"); },
#if UNITY_EDITOR
            true, // Keep heavy check on editor
#else
            false,
#endif
            config.spawnCapacity, config.spawnCapacityMax
        )
        {
            
            // init number of spawn at start in here
            List<GameObject> objSpawn = new List<GameObject>();

            for (int i = 0; i < config.spawnAtStart; i++)
                objSpawn.Add(Get());

            for (int i = 0; i < objSpawn.Count; i++)
                Release(objSpawn[i]);

            _config = config;
        }
    }
}
