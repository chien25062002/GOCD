using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GOCD.Framework.Optimize
{
    /// <summary>
    /// Base class giúp tự động connect collider với Optimize_ColliderLookup<T>.
    /// Không GetComponent ở runtime.
    /// </summary>
    public abstract class Optimize_CollidableBase<T> : MonoBehaviour where T : class
    {
        [SerializeField] protected Collider[] _colliders;

        // ====================== Odin Buttons ======================
#if UNITY_EDITOR
        [Button]
        void Load()
        {
            _colliders = GetComponentsInChildren<Collider>(true)
                .Where(c => c != null)
                .ToArray();
        }

        protected virtual void OnValidate()
        {
            if (!Application.isPlaying)
            {
                _colliders = GetComponentsInChildren<Collider>(true)
                    .Where(c => c != null)
                    .ToArray();
            }
        }
#endif
        // ===========================================================

        // ====================== Runtime ============================
        protected virtual void OnEnable()
        {
            if (_colliders != null && _colliders.Length > 0)
                Optimize_ColliderLookup<T>.Register((T)(object)this, _colliders);
        }

        protected virtual void OnDisable()
        {
            if (_colliders != null && _colliders.Length > 0)
                Optimize_ColliderLookup<T>.Unregister(_colliders);
        }
        // ===========================================================
    }
}
