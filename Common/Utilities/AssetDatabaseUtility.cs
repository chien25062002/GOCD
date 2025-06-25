#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Game
{
    public static class AssetDatabaseUtility
    {
        public static GameObject CloneAsPrefab(GameObject objectToClone)
        {
            GameObject originPrefab =(GameObject)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(objectToClone), typeof(GameObject));
            GameObject objSource = PrefabUtility.InstantiatePrefab(originPrefab) as GameObject;
            return objSource;
        }
        
        public static GameObject CloneAsPrefab(GameObject objectToClone, Transform parent, bool worldPositionStay = false)
        {
            GameObject originPrefab =(GameObject)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(objectToClone), typeof(GameObject));
            GameObject objSource = PrefabUtility.InstantiatePrefab(originPrefab) as GameObject;
            objSource.transform.SetParent(parent, worldPositionStay);
            return objSource;
        }
    }
}
#endif
