#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json.Linq;

public class GOCDDependencyInstallerWindow : EditorWindow
{
    private static readonly Dictionary<string, string> RequiredDependencies = new()
    {
        { "com.primetween.core", "https://github.com/KyryloKuzyk/PrimeTween.git" },
    };

    private Vector2 scroll;

    [MenuItem("GOCD/Dependency Installer")]
    public static void ShowWindow()
    {
        GetWindow<GOCDDependencyInstallerWindow>("GOCD Dependencies");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("GOCD Framework - Dependency Installer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (var dep in RequiredDependencies)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(dep.Key, GUILayout.Width(250));
            EditorGUILayout.TextField(dep.Value);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        if (GUILayout.Button("Install All Dependencies"))
        {
            InstallAllDependencies();
        }
    }

    private void InstallAllDependencies()
    {
        string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
        if (!File.Exists(manifestPath))
        {
            Debug.LogError("[GOCD] manifest.json not found!");
            return;
        }

        var json = File.ReadAllText(manifestPath);
        var manifest = JObject.Parse(json);
        var deps = manifest["dependencies"] as JObject;

        bool changed = false;
        foreach (var dep in RequiredDependencies)
        {
            if (deps[dep.Key] == null)
            {
                deps[dep.Key] = dep.Value;
                Debug.Log($"[GOCD] Added dependency: {dep.Key} -> {dep.Value}");
                changed = true;
            }
            else
            {
                Debug.Log($"[GOCD] Dependency already present: {dep.Key}");
            }
        }

        if (changed)
        {
            File.WriteAllText(manifestPath, manifest.ToString());
            AssetDatabase.Refresh();
            Debug.Log("[GOCD] Dependencies installed. Unity will reimport...");
        }
        else
        {
            Debug.Log("[GOCD] All dependencies are already installed.");
        }
    }
}
#endif
