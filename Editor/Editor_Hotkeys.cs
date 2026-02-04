#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class Editor_Hotkeys
{
    static Editor_Hotkeys()
    {
        // Khởi tạo khi Unity mở, không cần code trong đây nếu không dùng
    }

    // Ctrl + Shift + Number
    [MenuItem("Tools/⚙QuickScene/Open Scene 1 #1", false, int.MaxValue)]
    static void OpenScene1() => OpenSceneByBuildIndex(0);

    [MenuItem("Tools/⚙QuickScene/Open Scene 2 #2", false, int.MaxValue)]
    static void OpenScene2() => OpenSceneByBuildIndex(1);

    [MenuItem("Tools/⚙QuickScene/Open Scene 3 #3", false, int.MaxValue)]
    static void OpenScene3() => OpenSceneByBuildIndex(2);

    [MenuItem("Tools/⚙QuickScene/Open Scene 4 #4", false, int.MaxValue)]
    static void OpenScene4() => OpenSceneByBuildIndex(3);

    [MenuItem("Tools/⚙QuickScene/Open Scene 5 #5", false, int.MaxValue)]
    static void OpenScene5() => OpenSceneByBuildIndex(4);

    [MenuItem("Tools/⚙QuickScene/Open Scene 6 #6", false, int.MaxValue)]
    static void OpenScene6() => OpenSceneByBuildIndex(5);

    [MenuItem("Tools/⚙QuickScene/Open Scene 7 #7", false, int.MaxValue)]
    static void OpenScene7() => OpenSceneByBuildIndex(6);

    [MenuItem("Tools/⚙QuickScene/Open Scene 8 #8", false, int.MaxValue)]
    static void OpenScene8() => OpenSceneByBuildIndex(7);

    [MenuItem("Tools/⚙QuickScene/Open Scene 9 #9", false, int.MaxValue)]
    static void OpenScene9() => OpenSceneByBuildIndex(8);

    static void OpenSceneByBuildIndex(int index)
    {
        var scenes = EditorBuildSettings.scenes;
        if (index >= scenes.Length)
        {
            Debug.LogWarning($"❌ Không có scene ở index {index} trong Build Settings.");
            return;
        }

        string path = scenes[index].path;
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(path);
            Debug.Log($"✅ Đã mở scene: {path}");
        }
    }
}
#endif