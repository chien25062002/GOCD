#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;

namespace CFramework
{
    public class UnusedTexturesWindow : EditorWindow
    {
        private ReorderableList reorderableList;
        private List<string> unusedTextures = new List<string>();
        private string selectedFolder = "Assets";
        private Vector2 scrollPosition;

        [MenuItem("Tools/Find Unused Textures")]
        public static void ShowWindow()
        {
            GetWindow<UnusedTexturesWindow>("Unused Textures Finder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Select Folder to Search", EditorStyles.boldLabel);
            if (GUILayout.Button("Select Folder"))
            {
                string folder = EditorUtility.OpenFolderPanel("Select Folder", "Assets", "");
                if (!string.IsNullOrEmpty(folder))
                {
                    selectedFolder = "Assets" + folder.Replace(Application.dataPath, "");
                    FindUnusedTexturesInFolder(selectedFolder);
                    CreateReorderableList();
                }
            }

            if (reorderableList != null)
            {
                // Add scroll view
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                reorderableList.DoLayoutList();
                EditorGUILayout.EndScrollView();
            }

            if (GUILayout.Button("Refresh"))
            {
                FindUnusedTexturesInFolder(selectedFolder);
                reorderableList.list = unusedTextures;
            }

            if (GUILayout.Button("Delete All Unused Textures"))
            {
                DeleteAllUnusedTextures();
                FindUnusedTexturesInFolder(selectedFolder);
                reorderableList.list = unusedTextures;
            }
        }

        private void CreateReorderableList()
        {
            reorderableList = new ReorderableList(unusedTextures, typeof(string), true, true, false, true);
            reorderableList.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "Unused Textures"); };
            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                rect.y += 2;
                Rect objectFieldRect = new Rect(rect.x, rect.y, rect.width - 30, EditorGUIUtility.singleLineHeight);
                string assetPath = unusedTextures[index];
                Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
                EditorGUI.ObjectField(objectFieldRect, texture, typeof(Texture), false);
                Rect buttonRect = new Rect(rect.x + rect.width - 25, rect.y, 25, EditorGUIUtility.singleLineHeight);
                if (GUI.Button(buttonRect, "X"))
                {
                    DeleteTexture(assetPath);
                    unusedTextures.RemoveAt(index);
                }
            };
        }

        private void FindUnusedTexturesInFolder(string folder)
        {
            unusedTextures.Clear();
            string[] allTextures = AssetDatabase.FindAssets("t:Texture", new[] { folder });
            List<string> allPrefabsAndScenes = new List<string>(AssetDatabase.FindAssets("t:Prefab t:Scene"));

            foreach (string guid in allTextures)
            {
                string texturePath = AssetDatabase.GUIDToAssetPath(guid);
                Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(texturePath);
                if (texture != null)
                {
                    bool isUsed = false;

                    foreach (string assetGuid in allPrefabsAndScenes)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                        string[] dependencies = AssetDatabase.GetDependencies(assetPath);

                        if (Array.Exists(dependencies, dependency => dependency == texturePath))
                        {
                            isUsed = true;
                            break;
                        }
                    }

                    if (!isUsed)
                    {
                        unusedTextures.Add(texturePath);
                    }
                }
            }

            Debug.Log("Found " + unusedTextures.Count + " unused textures.");
        }

        private void DeleteTexture(string path)
        {
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.Refresh();
        }

        private void DeleteAllUnusedTextures()
        {
            foreach (string texturePath in unusedTextures)
            {
                AssetDatabase.DeleteAsset(texturePath);
            }

            AssetDatabase.Refresh();
        }
    }
}
#endif
