#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CFramework.Editor
{
    public class WindowFindGameobjectContainLayermask : EditorWindow
    {
        string layerToSearch = "Default";
        List<GameObject> foundObjects = new List<GameObject>();
        ReorderableList reorderableList;
        Vector2 scrollPos;

        [MenuItem("Tools/Window/Find Gameobject Contain Layermask")]
        public static void ShowWindow()
        {
            GetWindow<WindowFindGameobjectContainLayermask>("Layer Finder");
        }

        void OnEnable()
        {
            reorderableList = new ReorderableList(foundObjects, typeof(GameObject), true, true, true, true)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Found GameObjects");
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    foundObjects[index] = (GameObject)EditorGUI.ObjectField(
                        new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        foundObjects[index], typeof(GameObject), true);
                },
                onAddCallback = (ReorderableList list) =>
                {
                    foundObjects.Add(null);
                },
                onRemoveCallback = (ReorderableList list) =>
                {
                    foundObjects.RemoveAt(list.index);
                }
            };
        }

        void OnGUI()
        {
            GUILayout.Label("Layer Finder", EditorStyles.boldLabel);

            layerToSearch = EditorGUILayout.TextField("Layer to Search", layerToSearch);

            if (GUILayout.Button("Find GameObjects"))
            {
                FindGameObjectsWithLayer();
            }

            if (foundObjects != null && foundObjects.Count > 0)
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                reorderableList.DoLayoutList();
                EditorGUILayout.EndScrollView();
            }
            else if (foundObjects != null)
            {
                GUILayout.Label("No GameObjects found with layer: " + layerToSearch);
            }
        }

        void FindGameObjectsWithLayer()
        {
            foundObjects.Clear();

            int layer = LayerMask.NameToLayer(layerToSearch);
            if (layer == -1)
            {
                Debug.LogWarning("Layer not found: " + layerToSearch);
                return;
            }

            // Find GameObjects with layer in the active scene
            GameObject[] sceneObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in sceneObjects)
            {
                if (obj.layer == layer)
                {
                    foundObjects.Add(obj);
                }
            }

            // Find prefabs with layer in the project
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    FindLayeredChildren(prefab.transform, layer);
                }
            }
        }

        void FindLayeredChildren(Transform parent, int layer)
        {
            if (parent.gameObject.layer == layer)
            {
                foundObjects.Add(parent.gameObject);
            }

            foreach (Transform child in parent)
            {
                FindLayeredChildren(child, layer);
            }
        }
    }
}
#endif
