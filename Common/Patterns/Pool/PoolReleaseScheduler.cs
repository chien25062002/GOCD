using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GOCD.Framework
{
    /// <summary>
    /// Scheduler đẩy các instance "bẩn" chạy OnBeforeReleaseToPool (+ async) rồi trả về pool.
    /// - Enqueue(prefab, instance): đưa vào hàng đợi (de-dupe theo instance).
    /// - FlushAsync(): xử lý toàn bộ queue ngay (có nhả frame tránh hitch).
    /// - PendingCount: số item còn lại trong queue.
    /// - ClearQueue(): dọn sạch queue (dùng khi đổi scene).
    /// </summary>
    public static class PoolReleaseScheduler
    {
        // Số item xử lý mỗi frame trong vòng lặp nền
        const int ItemsPerFrame = 16;

        static readonly Queue<(GameObject prefab, GameObject instance)> _queue = new();
        static readonly HashSet<GameObject> _inQueue   = new(); // de-dupe theo instance
        static readonly HashSet<GameObject> _cancelled = new(); // đánh dấu huỷ release (tái sử dụng ngay)

        static bool _running;

        [RuntimeInitializeOnLoadMethod]
        static void Boot() => StartLoopIfNeeded();

        static void StartLoopIfNeeded()
        {
            if (_running) return;
            RunLoop().Forget();
        }

        /// <summary>
        /// Số lượng item đang chờ xử lý trong scheduler.
        /// </summary>
        public static int PendingCount => _queue.Count;

        /// <summary>
        /// Thêm instance cần release vào queue (idempotent).
        /// Bỏ qua nếu instance đã ở pool hoặc đã có trong queue.
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
        /// Huỷ lịch release của 1 instance (nếu đang trong queue) để tái sử dụng ngay.
        /// </summary>
        public static bool Cancel(GameObject instance)
        {
            if (!instance) return false;
            if (!_inQueue.Contains(instance)) return false;
            _cancelled.Add(instance);
            return true;
        }

        /// <summary>
        /// Dọn toàn bộ queue/cancel flags (dùng khi đổi scene).
        /// </summary>
        public static void ClearQueue()
        {
            _queue.Clear();
            _inQueue.Clear();
            _cancelled.Clear();
        }

        /// <summary>
        /// Kiểm tra instance có đang nằm trong queue release không.
        /// </summary>
        public static bool IsEnqueued(GameObject instance) => instance && _inQueue.Contains(instance);

        /// <summary>
        /// Xử lý toàn bộ queue NGAY (blocking theo thời gian logic, nhưng vẫn nhả frame để tránh hitch).
        /// Dùng trước khi Load scene: await PoolReleaseScheduler.FlushAsync();
        /// </summary>
        /// <param name="maxPerFrame">Giới hạn item xử lý mỗi frame để tránh hitch (mặc định 128 ở chỗ gọi Global)</param>
        public static async UniTask FlushAsync(int maxPerFrame = 128)
        {
            int processedThisFrame = 0;

            while (_queue.Count > 0)
            {
                var (prefab, inst) = _queue.Dequeue();

                if (!inst) { _inQueue.Remove(inst); continue; }

                // Nếu đã bị huỷ release (muốn tái dùng) → bỏ qua
                if (_cancelled.Remove(inst)) { _inQueue.Remove(inst); continue; }

                // Gọi các OnBeforeReleaseToPool (sync + optional async)
                await PreReleaseValidate(inst);

                if (!inst) { _inQueue.Remove(inst); continue; }
                if (PoolPrefabGlobal.IsReleased(inst)) { _inQueue.Remove(inst); continue; }

                // Đi lại qua Global để bookkeeping + release thật
                PoolPrefabGlobal.Release(prefab, inst);

                _inQueue.Remove(inst);

                // Nhả CPU mỗi "lát" để tránh hitch
                processedThisFrame++;
                if (processedThisFrame >= maxPerFrame)
                {
                    processedThisFrame = 0;
                    await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
                }
            }
        }

        /// <summary>
        /// Vòng lặp nền: mỗi frame xử lý một budget nhỏ trong queue.
        /// </summary>
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

                        if (_cancelled.Remove(inst)) { _inQueue.Remove(inst); continue; }

                        await PreReleaseValidate(inst);

                        if (!inst) { _inQueue.Remove(inst); continue; }
                        if (PoolPrefabGlobal.IsReleased(inst)) { _inQueue.Remove(inst); continue; }

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

        /// <summary>
        /// Gọi OnBeforeReleaseToPool() trên tất cả IPoolPreRelease (và OnBeforeReleaseToPoolAsync nếu có).
        /// Chỉ gọi trên handler đang Dirty.
        /// </summary>
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
