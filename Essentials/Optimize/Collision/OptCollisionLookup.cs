using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeSketch.Optimize
{
    public static class OptCollisionLookup
    {
        // Collider → (Type → List<Owner>)
        static readonly Dictionary<Collider, Dictionary<Type, List<object>>> _map = new();

        // =========================================================
        // REGISTER
        // =========================================================

        public static void Register(Type type, object owner, Collider[] colliders)
        {
            if (type == null || owner == null || colliders == null)
                return;

            foreach (var col in colliders)
            {
                if (!col) continue;

                if (!_map.TryGetValue(col, out var typeMap))
                {
                    typeMap = new Dictionary<Type, List<object>>(4);
                    _map[col] = typeMap;
                }

                if (!typeMap.TryGetValue(type, out var list))
                {
                    list = new List<object>(2);
                    typeMap[type] = list;
                }

                if (!list.Contains(owner)) // tránh double register
                    list.Add(owner);
            }
        }

        public static void Unregister(Type type, object owner, Collider[] colliders)
        {
            if (type == null || owner == null || colliders == null)
                return;

            foreach (var col in colliders)
            {
                if (!col) continue;
                if (!_map.TryGetValue(col, out var typeMap)) continue;
                if (!typeMap.TryGetValue(type, out var list)) continue;

                list.Remove(owner);

                if (list.Count == 0)
                    typeMap.Remove(type);

                if (typeMap.Count == 0)
                    _map.Remove(col);
            }
        }

        // =========================================================
        // QUERY – HOT PATH
        // =========================================================

        public static void ForEach<T>(Collider collider, Action<T> action)
            where T : class
        {
            if (!collider || action == null)
                return;

            if (!_map.TryGetValue(collider, out var typeMap))
                return;

            if (!typeMap.TryGetValue(typeof(T), out var list))
                return;

            foreach (var obj in list)
            {
                if (obj is T match)
                    action(match);
            }
        }

        // =========================================================

        public static void Clear()
        {
            _map.Clear();
        }
    }
}
