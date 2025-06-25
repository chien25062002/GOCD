#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CFramework
{
    public class MissingScriptUtility : EditorWindow
    {
        bool includeInactive = true;
        bool includePrefabs = true;

        [MenuItem("Window/MissingScriptUtility")]
        public static void ShowWindow()
        {
            GetWindow(typeof(MissingScriptUtility));
        }

        public void OnGUI()
        {
            var includeInactiveTooltip = "Whether to include inactive GameObjects in the search.";
            includeInactive = EditorGUILayout.Toggle(new GUIContent("Include Inactive", includeInactiveTooltip),
                includeInactive);

            var includePrefabsTooltip = "Whether to include prefab GameObjects in the search.";
            includePrefabs =
                EditorGUILayout.Toggle(new GUIContent("Include Prefabs", includePrefabsTooltip), includePrefabs);

            if (GUILayout.Button("Log Missing Scripts"))
                LogMissingScripts(SceneManager.GetActiveScene().GetRootGameObjects());
            if (GUILayout.Button("Log Missing Scripts from Selected GameObjects"))
                LogMissingScripts(SelectedGameObjects(includeInactive, includePrefabs));

            EditorGUILayout.Space();

            if (GUILayout.Button("Select GameObjects with Missing Scripts"))
                SelectGameObjectsWithMissingScripts(SceneManager.GetActiveScene().GetRootGameObjects());

            EditorGUILayout.Space();

            if (GUILayout.Button("Remove Missing Scripts"))
                RemoveMissingScripts(SceneManager.GetActiveScene().GetRootGameObjects());
            if (GUILayout.Button("Remove Missing Scripts from Selected GameObjects"))
                RemoveMissingScripts(SelectedGameObjects(includeInactive, includePrefabs));
        }

        public static void LogMissingScripts(GameObject[] gameObjects)
        {
            var gameObjectCount = 0;
            var missingScriptCount = 0;
            foreach (GameObject gameObject in gameObjects)
            {
                missingScriptCount += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                ++gameObjectCount;
            }

            Debug.Log(string.Format("Searched {0} GameObjects and found {1} missing scripts.", gameObjectCount,
                missingScriptCount));
        }

        public static void SelectGameObjectsWithMissingScripts(GameObject[] gameObjects)
        {
            var selections = new List<GameObject>();

            foreach (GameObject gameObject in gameObjects)
                if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject) > 0)
                    selections.Add(gameObject);

            Selection.objects = selections.ToArray();
        }

        public static void RemoveMissingScripts(GameObject[] gameObjects)
        {
            var missingScriptCount = 0;
            foreach (GameObject gameObject in gameObjects)
            {
                var count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
                if (count > 0)
                {
                    Undo.RegisterCompleteObjectUndo(gameObject, "Remove missing scripts");
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
                    missingScriptCount += count;
                }
            }

            Debug.Log(string.Format("Searched {0} GameObjects and removed {1} missing scripts.", gameObjects.Length,
                missingScriptCount));
        }

        #region Sub-utilities

        public static GameObject[] SelectedGameObjects(bool includingInactive = true, bool includingPrefabs = true)
        {
            var selectedGameObjects = new List<GameObject>(Selection.gameObjects);
            foreach (GameObject selectedGameObject in Selection.gameObjects)
            {
                var childTransforms = selectedGameObject.GetComponentsInChildren<Transform>(includingInactive);
                foreach (Transform childTransform in childTransforms)
                    selectedGameObjects.Add(childTransform.gameObject);
                if (includingPrefabs)
                {
                    var prefabs = new HashSet<GameObject>();
                    PrefabInstances(selectedGameObject, prefabs);
                    selectedGameObjects.AddRange(prefabs);
                }
            }

            return selectedGameObjects.ToArray();
        }

        public static int RecursiveMissingScriptCount(GameObject gameObject)
        {
            var missingScriptCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);

            var childTransforms = gameObject.GetComponentsInChildren<Transform>(true);
            foreach (Transform childTransform in childTransforms)
                missingScriptCount +=
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(childTransform.gameObject);

            return missingScriptCount;
        }

        static void PrefabInstances(GameObject instance, HashSet<GameObject> prefabs)
        {
            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(instance);
            // Only visit if source is valid, and hasn't been visited before
            if (source == null || !prefabs.Add(source))
                return;

            PrefabInstances(source, prefabs);
        }

        #endregion
    }
}
#endif