using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GOCD.Framework
{
    public static class PoolReleaseScheduler
    {
        const int ItemsPerFrame = 16;
        static readonly Queue<(GameObject prefab, GameObject instance)> _queue = new();
        static bool _running;

        [RuntimeInitializeOnLoadMethod]
        static void Boot() => StartLoopIfNeeded();

        static void StartLoopIfNeeded()
        {
            if (_running) return;
            RunLoop().Forget();
        }

        public static void Enqueue(GameObject prefab, GameObject instance)
        {
            if (prefab == null || instance == null) return;
            _queue.Enqueue((prefab, instance));
            StartLoopIfNeeded();
        }

        public static void ClearQueue() => _queue.Clear();

        static async UniTaskVoid RunLoop()
        {
            _running = true;

            while (Application.isPlaying)
            {
                if (_queue.Count == 0)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update);
                    continue;
                }

                int budget = ItemsPerFrame;
                while (budget-- > 0 && _queue.Count > 0)
                {
                    var (prefab, inst) = _queue.Dequeue();
                    await PreReleaseValidate(inst);
                    PoolPrefabGlobal.GetPool(prefab).Release(inst);
                }

                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            _running = false;
        }

        static async UniTask PreReleaseValidate(GameObject go)
        {
            if (go == null) return;

            var handlers = go.GetComponentsInChildren<IPoolPreRelease>(true);
            for (int i = 0; i < handlers.Length; i++)
            {
                var h = handlers[i];
                if (h == null || !h.IsDirty) continue;

                h.OnBeforeReleaseToPool();

                if (h is IAsyncPoolPreRelease asyncH)
                    await asyncH.OnBeforeReleaseToPoolAsync();
            }
        }
    }
}
