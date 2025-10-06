#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using GOCD.Framework.Internet;
using UnityEditor.Build;

namespace GOCD.Framework.Editor
{
    public class Window_GOCDFramework : EditorWindow
    {
        const string INTERNET_DEFINE = "INTERNET_CHECK";

        int _tab = 0;
        static readonly string[] _tabs = { "Internet" };

        InternetSettings _settings;

        [MenuItem("GOCD/Settings")]
        public static void Open()
        {
            var w = GetWindow<Window_GOCDFramework>("GOCD Framework");
            w.minSize = new Vector2(520, 340);
            w.Show();
        }

        void OnEnable()
        {
            // Tìm asset mặc định
            _settings = FindSettings();
        }

        void OnGUI()
        {
            EditorGUILayout.Space();
            _tab = GUILayout.Toolbar(_tab, _tabs, GUILayout.Height(24));
            EditorGUILayout.Space();

            switch (_tab)
            {
                case 0:
                    DrawInternetTab();
                    break;
            }
        }

        void DrawInternetTab()
        {
            var group = EditorUserBuildSettings.selectedBuildTargetGroup;
            GetDefines(group, out var defines);

            // Define toggles
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Scripting Define Symbols", EditorStyles.boldLabel);

                bool hasInternet = defines.Contains(INTERNET_DEFINE);
                bool newHasInternet = EditorGUILayout.ToggleLeft($"Enable {INTERNET_DEFINE}", hasInternet);
                if (newHasInternet != hasInternet)
                {
                    SetDefine(group, INTERNET_DEFINE, newHasInternet);
                }
            }

            EditorGUILayout.Space();

            // Settings asset
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Internet Settings", EditorStyles.boldLabel);
                _settings = (InternetSettings)EditorGUILayout.ObjectField("Asset", _settings, typeof(InternetSettings), false);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(_settings ? "Ping Asset" : "Find Asset"))
                    {
                        _settings = FindSettings();
                        Repaint();
                    }

                    if (GUILayout.Button("Create/Locate Default"))
                    {
                        _settings = CreateOrLocateDefaultSettings();
                        Repaint();
                    }
                }

                if (_settings)
                {
                    EditorGUI.BeginChangeCheck();

                    _settings.testUrl = EditorGUILayout.TextField("Test URL", _settings.testUrl);
                    _settings.requestTimeout =
                        EditorGUILayout.IntField("Request Timeout (s)", Mathf.Max(1, _settings.requestTimeout));
                    _settings.checkInterval =
                        EditorGUILayout.FloatField("Check Interval (s)", Mathf.Max(1f, _settings.checkInterval));

                    EditorGUILayout.Space();
                    _settings.internetViewPrefab = (GameObject)EditorGUILayout.ObjectField(
                        new GUIContent("Internet Popup Prefab"),
                        _settings.internetViewPrefab, typeof(GameObject), false);

                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(_settings);
                    }

                    EditorGUILayout.Space();

                    // Runtime util
                    using (new EditorGUI.DisabledScope(!Application.isPlaying))
                    {
                        EditorGUILayout.LabelField("Runtime Tools", EditorStyles.boldLabel);

                        if (GUILayout.Button("Open Popup (No Internet)"))
                        {
                            // Mở popup bất kể trạng thái mạng: giả lập offline
                            InternetHelper.CurrentSettings = _settings;
                            ForceOpenPopup();
                        }

                        if (GUILayout.Button("Close Popup"))
                        {
                            if (InternetHelper.InternetPopup) InternetHelper.InternetPopup.Close();
                        }

                        if (GUILayout.Button("Force Check Now"))
                        {
                            Internet_Checker.ForceCheckNow();
                        }

                        EditorGUILayout.LabelField(
                            "Status:",
                            $"IsInternetAvailable={InternetHelper.IsInternetAvailable}, " +
                            $"PopupOpen={(InternetHelper.InternetPopup && InternetHelper.InternetPopup.gameObject.activeInHierarchy)}"
                        );
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "Chưa có InternetSettings. Nhấn 'Create/Locate Default' để tạo ở Assets/Resources/GOCD/InternetSettings.asset",
                        MessageType.Info);
                }
            }
        }

        // ===== Helpers =====
        static InternetSettings FindSettings()
        {
            var path = $"Assets/Resources/{InternetSettings.DefaultResourcesPath}.asset";
            var obj = AssetDatabase.LoadAssetAtPath<InternetSettings>(path);
            if (obj) return obj;

            var guids = AssetDatabase.FindAssets("t:InternetSettings");
            if (guids != null && guids.Length > 0)
            {
                var p = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<InternetSettings>(p);
            }

            return null;
        }

        static InternetSettings CreateOrLocateDefaultSettings()
        {
            var obj = FindSettings();
            if (obj) return obj;

            string dir = "Assets/Resources/GOCD";
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder(dir))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "GOCD");
            }

            string path = $"{dir}/InternetSettings.asset";
            var inst = ScriptableObject.CreateInstance<InternetSettings>();
            AssetDatabase.CreateAsset(inst, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return inst;
        }

        static void GetDefines(BuildTargetGroup group, out string defines)
        {
#if UNITY_6000_0_OR_NEWER
            defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group));
#elif UNITY_2023_1_OR_NEWER
    defines = PlayerSettings.GetScriptingDefineSymbols(group);
#else
            defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
#endif
        }

        static void SetDefine(BuildTargetGroup group, string symbol, bool enable)
        {
            GetDefines(group, out var defines);
            var list = defines.Split(';');
            var set = new System.Collections.Generic.HashSet<string>(list);

            if (enable) set.Add(symbol);
            else set.Remove(symbol);

            var joined = string.Join(";", set);

#if UNITY_6000_0_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group), joined);
#elif UNITY_2023_1_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(group, joined);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, joined);
#endif
        }

        static void ForceOpenPopup()
        {
            if (!Application.isPlaying) return;

            var settings = InternetHelper.CurrentSettings;
            if (settings == null || settings.internetViewPrefab == null)
            {
                Debug.LogWarning("[GOCD] InternetSettings hoặc Internet Popup Prefab chưa được gán.");
                return;
            }

            // Đóng popup cũ nếu còn 
            if (InternetHelper.InternetPopup != null && InternetHelper.InternetPopup.gameObject)
                InternetHelper.InternetPopup.Close();

            // Mở popup trực tiếp bằng PopupManager (không cần check mạng)
            var popup = PopupManager.Create(settings.internetViewPrefab);
            InternetHelper.InternetPopup = popup;
            PopupManager.PushToStack(popup, isTopHidden: false);

            // Đánh dấu registry để tránh mở trùng nếu bấm nhiều lần
            InternetHelper.RegisterUI(popup.transform);
        }
    }
}
#endif
