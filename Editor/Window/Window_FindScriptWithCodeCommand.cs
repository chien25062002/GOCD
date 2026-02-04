#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CodeSketch.Editor
{
    public class Window_FindScriptWithCodeCommand : EditorWindow
    {
        string tagToSearch = "";
        string searchFolder = "Assets"; // Assets-relative path

        readonly List<string> foundScripts = new List<string>();
        ReorderableList reorderableList;
        Vector2 scrollPos;

        [MenuItem("CodeSketch/Tools/Window/Find Script With Code Command")]
        public static void ShowWindow()
        {
            GetWindow<Window_FindScriptWithCodeCommand>("Find Script With Code Command");
        }

        void OnEnable()
        {
            reorderableList = new ReorderableList(foundScripts, typeof(string), true, true, false, false)
            {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Scripts With Code Command");
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (index < 0 || index >= foundScripts.Count)
                        return;

                    string path = foundScripts[index];

                    Rect labelRect = new Rect(
                        rect.x,
                        rect.y,
                        rect.width,
                        EditorGUIUtility.singleLineHeight
                    );

                    // Hiển thị path
                    EditorGUI.LabelField(labelRect, path);

                    // Detect click
                    Event e = Event.current;
                    if (e.type == EventType.MouseDown && labelRect.Contains(e.mousePosition))
                    {
                        Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                        if (asset != null)
                        {
                            EditorGUIUtility.PingObject(asset);
                            AssetDatabase.OpenAsset(asset);
                        }

                        e.Use();
                    }
                }
            };
        }

        void OnGUI()
        {
            GUILayout.Label("Find Scripts With Code Command", EditorStyles.boldLabel);

            tagToSearch = EditorGUILayout.TextField("Tag to Search", tagToSearch);

            // Folder display + select button
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search Folder", GUILayout.Width(90));
            EditorGUILayout.SelectableLabel(searchFolder, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                SelectFolder();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                $"Search scope: {searchFolder}",
                MessageType.Info
            );

            if (GUILayout.Button("Find Scripts With Code Command"))
            {
                FindScriptsUsingTag();
            }

            if (foundScripts.Count > 0)
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                reorderableList.DoLayoutList();
                EditorGUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("No scripts found.");
            }
        }

        // =====================================================
        // FOLDER PICKER
        // =====================================================

        void SelectFolder()
        {
            string absPath = EditorUtility.OpenFolderPanel(
                "Select Folder To Search",
                Application.dataPath,
                ""
            );

            if (string.IsNullOrEmpty(absPath))
                return;

            // Normalize
            absPath = absPath.Replace('\\', '/');
            string dataPath = Application.dataPath.Replace('\\', '/');

            if (!absPath.StartsWith(dataPath))
            {
                EditorUtility.DisplayDialog(
                    "Invalid Folder",
                    "Please select a folder inside the Assets directory.",
                    "OK"
                );
                return;
            }

            // Convert absolute → Assets-relative
            searchFolder = "Assets" + absPath.Substring(dataPath.Length);
        }

        // =====================================================
        // SEARCH
        // =====================================================

        void FindScriptsUsingTag()
        {
            foundScripts.Clear();

            if (string.IsNullOrWhiteSpace(tagToSearch))
                return;

            string search = Sanitize(tagToSearch);

            string[] searchInFolders = { searchFolder };
            string[] scriptGuids = AssetDatabase.FindAssets("t:Script", searchInFolders);

            foreach (string guid in scriptGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!path.EndsWith(".cs"))
                    continue;

                try
                {
                    string content = File.ReadAllText(path, System.Text.Encoding.UTF8);
                    content = Sanitize(content);

                    if (content.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        foundScripts.Add(path);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Cannot read {path}: {e.Message}");
                }
            }
        }
        
        string Sanitize(string s)
        {
            return s
                .Trim()
                .Replace("\uFEFF", "")   // BOM
                .Replace("\u200B", "")   // zero-width space
                .Replace("\u00A0", " "); // non-breaking space
        }

    }
}
#endif
