using UnityEngine;

namespace GOCD.Framework.Optimize
{
    /// <summary>
    /// Base class generic cho mọi entity có thể bắt va chạm với loại collidable T.
    /// Tự động dispatch OnTrigger và OnCollision qua Optimize_ColliderLookup<T>.
    /// Không GetComponent, không GC, cực nhẹ.
    /// </summary>
    public abstract class Optimize_CollisionHandlerBase<T> : MonoCached
        where T : class
    {
        // ==================== COLLISION ====================
        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (Optimize_ColliderLookup<T>.TryGet(collision.collider, out var target))
                OnCollisionEnterFunc(target);
        }

        protected virtual void OnCollisionExit(Collision collision)
        {
            if (Optimize_ColliderLookup<T>.TryGet(collision.collider, out var target))
                OnCollisionExitFunc(target);
        }

        // ==================== TRIGGER ======================
        protected virtual void OnTriggerEnter(Collider other)
        {
            if (Optimize_ColliderLookup<T>.TryGet(other, out var target))
                OnTriggerEnterFunc(target);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (Optimize_ColliderLookup<T>.TryGet(other, out var target))
                OnTriggerExitFunc(target);
        }

        // ==================== ABSTRACT CALLBACKS =============
        protected abstract void OnCollisionEnterFunc(T target);
        protected abstract void OnCollisionExitFunc(T target);
        protected abstract void OnTriggerEnterFunc(T target);
        protected abstract void OnTriggerExitFunc(T target);
    }
}