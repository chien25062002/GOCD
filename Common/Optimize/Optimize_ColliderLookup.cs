using System.Collections.Generic;
using UnityEngine;

namespace GOCD.Framework.Optimize
{
    /// <summary>
    /// Cache collider → owner map cho 1 loại T.
    /// </summary>
    public static class Optimize_ColliderLookup<T> where T : class
    {
        static readonly Dictionary<Collider, T> _map = new();

        public static void Register(T owner, Collider[] colliders) 
        {
            if (colliders == null) return;
            foreach (var col in colliders)
            {
                if (col == null) continue;
                _map[col] = owner;
            }
        }
        
        public static void Register(T owner, Collider collider) 
        {
            if (collider == null) return;
            _map[collider] = owner;
        }

        public static void Unregister(Collider[] colliders) 
        {
            if (colliders == null) return;
            foreach (var col in colliders) {
                if (col == null) continue;
                _map.Remove(col);
            }
        }
        
        public static void Unregister(Collider collider) 
        {
            if (collider == null) return;
            _map.Remove(collider);
        }

        public static bool TryGet(Collider col, out T owner) 
        {
            return _map.TryGetValue(col, out owner);
        }
    }
}
