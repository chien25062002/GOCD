using System;
using System.Collections.Generic;
using UnityEngine;

namespace GOCD.Framework
{
    public class MonoCachedComponent : MonoBase
    {
        readonly Dictionary<Type, Component> _localCache = new();
        readonly Dictionary<Type, Component> _parentCache = new();
        readonly Dictionary<Type, Component> _childrenCache = new();
        
        /// <summary>
        /// Lấy component từ chính object này (có cache)
        /// </summary>
        public T GetCachedComponent<T>() where T : Component
        {
            var type = typeof(T);
            if (!_localCache.TryGetValue(type, out var comp) || comp == null)
            {
                comp = GetComponent<T>();
                _localCache[type] = comp;
            }

            return (T)comp;
        }

        /// <summary>
        /// Lấy component từ object cha gần nhất (có cache)
        /// </summary>
        public T GetCachedComponentInParent<T>() where T : Component
        {
            var type = typeof(T);
            if (!_parentCache.TryGetValue(type, out var comp) || comp == null)
            {
                comp = GetComponentInParent<T>();
                _parentCache[type] = comp;
            }

            return (T)comp;
        }

        /// <summary>
        /// Lấy component từ object con gần nhất (có cache)
        /// </summary>
        public T GetCachedComponentInChildren<T>() where T : Component
        {
            var type = typeof(T);
            if (!_childrenCache.TryGetValue(type, out var comp) || comp == null)
            {
                comp = GetComponentInChildren<T>();
                _childrenCache[type] = comp;
            }

            return (T)comp;
        }
    }
}
