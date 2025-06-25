#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CFramework.Editor
{
    public class WindowFindScriptUseTagName : EditorWindow
    {
        string tagToSearch = "tool";
        List<string> foundScripts = new List<string>();
        ReorderableList reorderableList;
        Vector2 scrollPos;

        [MenuItem("Tools/Window/Tag Usage Finder")]
        public static void ShowWindow()
        {
            GetWindow<WindowFindScriptUseTagName>("Tag Usage Finder");
        }

        void OnEnable()
        {
            reorderableList = new ReorderableList(foundScripts, typeof(string), true, true, false, false)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    EditorGUI.LabelField(rect, "Scripts Using Tag");
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), foundScripts[index]);
                }
            };
        }

        void OnGUI()
        {
            GUILayout.Label("Tag Usage Finder", EditorStyles.boldLabel);

            tagToSearch = EditorGUILayout.TextField("Tag to Search", tagToSearch);

            if (GUILayout.Button("Find Scripts Using Tag"))
            {
                FindScriptsUsingTag();
            }

            if (foundScripts != null && foundScripts.Count > 0)
            {
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                reorderableList.DoLayoutList();
                EditorGUILayout.EndScrollView();
            }
            else if (foundScripts != null)
            {
                GUILayout.Label("No scripts found using tag: " + tagToSearch);
            }
        }

        void FindScriptsUsingTag()
        {
            foundScripts.Clear();

            // Find all script files in the project
            string[] scriptGuids = AssetDatabase.FindAssets("t:Script");
            foreach (string guid in scriptGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string scriptContent = File.ReadAllText(path);

                if (scriptContent.Contains("\"" + tagToSearch + "\""))
                {
                    foundScripts.Add(path);
                }
            }
        }
    }
}
#endif
