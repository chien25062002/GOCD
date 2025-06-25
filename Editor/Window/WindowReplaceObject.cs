#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFramework.Editor
{
    public class WindowReplaceObject : EditorWindow
    {
        private List<GameObject> _objectsToReplace = new();
        private GameObject _replacementPrefab;
        private bool _destroyOldObject = true; // 👈 Tuỳ chọn Destroy old object
        private Vector2 _scrollPosition;       // 👈 Scroll position

        [MenuItem("Tools/Window/Replace Objects")]
        public static void ShowWindow()
        {
            GetWindow<WindowReplaceObject>("Replace Objects");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Danh sách GameObjects để Replace", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawDragDropArea();

            EditorGUILayout.Space();

            // Bắt đầu ScrollView
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200)); // 👈 Set chiều cao scroll view

            if (_objectsToReplace != null)
            {
                for (int i = 0; i < _objectsToReplace.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    _objectsToReplace[i] = (GameObject)EditorGUILayout.ObjectField(_objectsToReplace[i], typeof(GameObject), true);
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        _objectsToReplace.RemoveAt(i);
                        i--;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView(); // Kết thúc ScrollView

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Prefab thay thế", EditorStyles.boldLabel);
            _replacementPrefab = (GameObject)EditorGUILayout.ObjectField(_replacementPrefab, typeof(GameObject), false);

            EditorGUILayout.Space();

            _destroyOldObject = EditorGUILayout.Toggle("Destroy Object cũ sau khi Replace", _destroyOldObject);

            EditorGUILayout.Space();

            if (GUILayout.Button("Replace Selected Objects"))
            {
                if (_replacementPrefab == null)
                {
                    Debug.LogError("Bạn chưa chọn Prefab để thay thế!");
                    return;
                }

                ReplaceObjects();
            }
        }

        private void DrawDragDropArea()
        {
            Rect dropArea = GUILayoutUtility.GetRect(0f, 80f, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Kéo thả GameObjects vào đây", EditorStyles.helpBox);

            Event evt = Event.current;

            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                if (dropArea.Contains(evt.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        foreach (var draggedObject in DragAndDrop.objectReferences)
                        {
                            if (draggedObject is GameObject go && !_objectsToReplace.Contains(go))
                            {
                                _objectsToReplace.Add(go);
                            }
                        }
                    }

                    evt.Use();
                }
            }
        }

        private void ReplaceObjects()
        {
            Undo.RegisterCompleteObjectUndo(this, "Replace Objects");

            List<GameObject> newObjects = new();

            foreach (GameObject obj in _objectsToReplace)
            {
                if (obj == null)
                    continue;

                Transform originalTransform = obj.transform;
                string originalName = obj.name;
                Transform parent = originalTransform.parent;

                // Instantiate prefab
                GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(_replacementPrefab, parent);

                // Copy Transform
                newObj.transform.position = originalTransform.position;
                newObj.transform.rotation = originalTransform.rotation;
                newObj.transform.localScale = originalTransform.localScale;

                // Copy Name
                newObj.name = originalName;

                Undo.RegisterCreatedObjectUndo(newObj, "Create Replacement");

                if (_destroyOldObject)
                {
                    Undo.DestroyObjectImmediate(obj);
                }
                else
                {
                    obj.SetActive(false);
                }

                newObjects.Add(newObj);
            }

            _objectsToReplace.Clear();

            Debug.Log($"Đã replace {newObjects.Count} đối tượng.");
        }
    }
}
#endif
