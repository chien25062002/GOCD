#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CFramework.Editor
{
    public class TagFinderWindow : EditorWindow
    {
        private string tagToSearch = "tool";
        private List<GameObject> foundObjects = new List<GameObject>();
        private ReorderableList reorderableList;
        private Vector2 scrollPos;

        [MenuItem("Tools/Tag Finder")]
        public static void ShowWindow()
        {
            GetWindow<TagFinderWindow>("Tag Finder");
        }

        private void OnEnable()
        {
            reorderableList = new ReorderableList(foundObjects, typeof(GameObject), true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Found GameObjects"); },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    foundObjects[index] = (GameObject)EditorGUI.ObjectField(
                        new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        foundObjects[index], typeof(GameObject), true);
                },
                onAddCallback = (ReorderableList list) => { foundObjects.Add(null); },
                onRemoveCallback = (ReorderableList list) => { foundObjects.RemoveAt(list.index); }
            };
        }

        private void OnGUI()
        {
            GUILayout.Label("Tag Finder", EditorStyles.boldLabel);

            tagToSearch = EditorGUILayout.TextField("Tag to Search", tagToSearch);

            if (GUILayout.Button("Find GameObjects"))
            {
                FindGameObjectsWithTag();
            }

            if (foundObjects != null && foundObjects.Count > 0)
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                reorderableList.DoLayoutList();
                EditorGUILayout.EndScrollView();
            }
            else if (foundObjects != null)
            {
                GUILayout.Label("No GameObjects found with tag: " + tagToSearch);
            }
        }

        private void FindGameObjectsWithTag()
        {
            foundObjects.Clear();

            // Find GameObjects with tag in the active scene
            GameObject[] sceneObjects = GameObject.FindGameObjectsWithTag(tagToSearch);
            foundObjects.AddRange(sceneObjects);

            // Find prefabs with tag in the project
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    FindTaggedChildren(prefab.transform);
                }
            }
        }

        private void FindTaggedChildren(Transform parent)
        {
            if (parent.CompareTag(tagToSearch))
            {
                foundObjects.Add(parent.gameObject);
            }

            foreach (Transform child in parent)
            {
                FindTaggedChildren(child);
            }
        }
    }
}
#endif
