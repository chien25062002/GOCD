using UnityEngine;

using CodeSketch.Mono;

namespace CodeSketch.Optimize
{
    /// <summary>
    /// Base class cho entity xử lý va chạm với các đối tượng đã
    /// register vào CSKOpt_ColliderLookup theo type T.
    ///
    /// Đặc điểm:
    /// - Không GetComponent
    /// - Không GC runtime
    /// - Hỗ trợ 1 collider → nhiều T
    /// - An toàn với pooling / enable-disable
    /// </summary>
    public abstract class OptCollisionSensor<T> : MonoBase
        where T : class
    {
        // ==================== COLLISION ====================

        protected virtual void OnCollisionEnter(Collision collision)
        {
            var col = collision.collider;
            if (!col) return;

            OptCollisionLookup.ForEach<T>(
                col,
                OnCollisionEnterFunc
            );
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
            var col = collision.collider;
            if (!col) return;

            OptCollisionLookup.ForEach<T>(
                col,
                OnCollisionExitFunc
            );
        }

        // ==================== TRIGGER ======================

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!other) return;

            OptCollisionLookup.ForEach<T>(
                other,
                OnTriggerEnterFunc
            );
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (!other) return;

            OptCollisionLookup.ForEach<T>(
                other,
                OnTriggerExitFunc
            );
        }

        // ==================== GAMEPLAY CALLBACKS ============

        protected abstract void OnCollisionEnterFunc(T target);
        protected abstract void OnCollisionExitFunc(T target);
        protected abstract void OnTriggerEnterFunc(T target);
        protected abstract void OnTriggerExitFunc(T target);
    }
}