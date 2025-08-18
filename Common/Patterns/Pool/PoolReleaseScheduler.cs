using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GOCD.Framework
{
    public static class PoolReleaseScheduler
    {
        const int ItemsPerFrame = 16;

        static readonly Queue<(GameObject prefab, GameObject instance)> _queue = new();
        static readonly HashSet<GameObject> _inQueue   = new(); // de-dupe theo instance
        static readonly HashSet<GameObject> _cancelled = new(); // đánh dấu huỷ release

        static bool _running;

        [RuntimeInitializeOnLoadMethod]
        static void Boot() => StartLoopIfNeeded();

        static void StartLoopIfNeeded()
        {
            if (_running) return;
            RunLoop().Forget();
        }

        /// <summary>
        /// Thêm instance cần release vào hàng đợi. Nếu instance đã ở pool hoặc đã nằm trong hàng đợi thì bỏ qua.
        /// </summary>
        public static void Enqueue(GameObject prefab, GameObject instance)
        {
            if (prefab == null || instance == null) return;
            if (PoolPrefabGlobal.IsReleased(instance)) return; // đã ở pool rồi
            if (!_inQueue.Add(instance)) return;               // đã trong queue

            _queue.Enqueue((prefab, instance));
            StartLoopIfNeeded();
        }

        /// <summary>
        /// Huỷ lịch release của 1 instance nếu bạn muốn tái sử dụng ngay lập tức.
        /// </summary>
        public static bool Cancel(GameObject instance)
        {
            if (!instance) return false;
            if (!_inQueue.Contains(instance)) return false;
            _cancelled.Add(instance);
            return true;
        }

        /// <summary>
        /// Dọn toàn bộ queue/cancel flags.
        /// </summary>
        public static void ClearQueue()
        {
            _queue.Clear();
            _inQueue.Clear();
            _cancelled.Clear();
        }

        /// <summary>
        /// (Optional) Kiểm tra instance có đang nằm trong queue release không.
        /// </summary>
        public static bool IsEnqueued(GameObject instance) => instance && _inQueue.Contains(instance);

        static async UniTaskVoid RunLoop()
        {
            _running = true;
            try
            {
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

                        if (!inst) { _inQueue.Remove(inst); continue; }

                        // Nếu đã bị huỷ release (vì muốn tái sử dụng) → bỏ qua
                        if (_cancelled.Remove(inst)) { _inQueue.Remove(inst); continue; }

                        // Gọi các OnBeforeReleaseToPool (có thể await)
                        await PreReleaseValidate(inst);

                        if (!inst) { _inQueue.Remove(inst); continue; }
                        if (PoolPrefabGlobal.IsReleased(inst)) { _inQueue.Remove(inst); continue; }

                        // QUAN TRỌNG: đi lại qua Global để bookkeeping (_releasedInstances) + release thật
                        PoolPrefabGlobal.Release(prefab, inst);

                        _inQueue.Remove(inst);
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }
            finally
            {
                _running = false;
            }
        }

        static async UniTask PreReleaseValidate(GameObject go)
        {
            if (!go) return;

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
