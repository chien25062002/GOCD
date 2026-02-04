using UnityEngine;

namespace CodeSketch.Optimize
{
    /// <summary>
    /// Base class:
    /// - Auto register colliders to CSKOpt_ColliderLookup
    /// - T = interface / base gameplay type
    /// - Zero GetComponent in runtime
    /// </summary>
    public abstract class OptCollisionRegister<T> : MonoBehaviour where T : class
    {
        [SerializeField]
        protected Collider[] _colliders;

        // =====================================================
        // LIFECYCLE
        // =====================================================

        protected virtual void Awake()
        {
            if (_colliders == null || _colliders.Length == 0)
            {
                _colliders = GetComponentsInChildren<Collider>(true);
            }
        }

        protected virtual void OnEnable()
        {
            if (_colliders == null || _colliders.Length == 0)
                return;

            OptCollisionLookup.Register(typeof(T), this, _colliders);
        }

        protected virtual void OnDisable()
        {
            if (_colliders == null || _colliders.Length == 0)
                return;

            OptCollisionLookup.Unregister(typeof(T), this, _colliders);
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (!Application.isPlaying)
            {
                _colliders = GetComponentsInChildren<Collider>(true);
            }
        }
#endif
    }
}