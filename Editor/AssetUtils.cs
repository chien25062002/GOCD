using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
#endif

namespace CFramework.Editor
{
    public static class AssetUtils
    {
#if UNITY_EDITOR
        public static List<T> LoadAllAssetsInFolder<T>(string folderPath) where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folderPath });
            var list = new List<T>();
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                    list.Add(asset);
            }

            return list;
        }
#endif
    }
}