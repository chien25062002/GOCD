using UnityEngine;

#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using OdinSerializer;
using UnityEditor;
using SerializationUtility = OdinSerializer.SerializationUtility;

namespace CodeSketch.Editor
{
    // EditorWindow nằm cùng namespace, tên khác để không trùng MonoBehaviour
    public class Window_DataEditting : EditorWindow
    {
        Vector2 _left, _right;
        string _root;
        FileInfo[] _files = Array.Empty<FileInfo>();
        int _idx = -1;

        object _obj;
        Type _type;
        string _fileName;

        readonly Dictionary<string, bool> _fold = new();
        readonly Dictionary<object, bool> _goRaw = new(); // toggle Raw _value cho GOCDValue<T>

        [MenuItem("CodeSketch/Data/Window Data Preview", false)]
        static void Open()
        {
            var w = GetWindow<Window_DataEditting>("CodeSketch Data Preview");
            w.minSize = new Vector2(900, 550);
            w.Init();
            w.Show();
        }

        void OnEnable() => Init();

        void Init()
        {
            _root = Path.Combine(Application.persistentDataPath, "CodeSketch/Data"); // data folder gốc
            Refresh();
        }

        void Refresh()
        {
            try
            {
                if (!Directory.Exists(_root))
                {
                    _files = Array.Empty<FileInfo>();
                    _idx = -1;
                    return;
                }
                _files = new DirectoryInfo(_root)
                    .GetFiles("*", SearchOption.TopDirectoryOnly)
                    .OrderByDescending(f => f.LastWriteTimeUtc).ToArray();
                if (_idx >= _files.Length) _idx = -1;
            }
            catch (Exception e)
            {
                Debug.LogError("[CodeSketch Data Viewer] Refresh failed: " + e);
                _files = Array.Empty<FileInfo>();
                _idx = -1;
            }
        }

        void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();

            // LEFT
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.42f));
            using (var sv = new EditorGUILayout.ScrollViewScope(_left, GUILayout.ExpandHeight(true)))
            {
                _left = sv.scrollPosition;
                DrawFiles();
            }
            EditorGUILayout.EndVertical();

            // RIGHT
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            using (var sv = new EditorGUILayout.ScrollViewScope(_right, GUILayout.ExpandHeight(true)))
            {
                _right = sv.scrollPosition;
                DrawInspector();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        void DrawHeader()
        {
            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Persistent Path:", GUILayout.Width(110));
                EditorGUILayout.SelectableLabel(Application.persistentDataPath, GUILayout.Height(18));
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Folder"))
                {
                    if (!Directory.Exists(_root)) Directory.CreateDirectory(_root);
                    EditorUtility.RevealInFinder(_root);
                }
                if (GUILayout.Button("Refresh")) Refresh();
                GUILayout.FlexibleSpace();
                if (_obj != null)
                {
                    if (GUILayout.Button("Save", GUILayout.Width(100))) Save();
                    if (GUILayout.Button("Reload", GUILayout.Width(100))) Load(_idx);
                }
            }
        }

        void DrawFiles()
        {
            EditorGUILayout.LabelField("CodeSketch Data Files", EditorStyles.boldLabel);

            if (_files.Length == 0)
            {
                EditorGUILayout.HelpBox($"No files found in:\n{_root}\n\nRun the game to generate files, then Refresh.", MessageType.Info);
                return;
            }

            for (int i = 0; i < _files.Length; i++)
            {
                var f = _files[i];
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    var head = f.Name;
                    var sub = $"{FmtBytes(f.Length)}  •  {f.LastWriteTime.ToLocalTime():yyyy-MM-dd HH:mm:ss}";
                    var style = new GUIStyle(EditorStyles.label);
                    if (i == _idx) style.fontStyle = FontStyle.Bold;

                    if (GUILayout.Button(head, style))
                    {
                        _idx = i; Load(i);
                    }
                    EditorGUILayout.LabelField(sub, EditorStyles.miniLabel);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Load", GUILayout.Width(80))) { _idx = i; Load(i); }
                        if (GUILayout.Button("Delete", GUILayout.Width(80)) &&
                            EditorUtility.DisplayDialog("Delete File?",
                                $"Delete '{f.Name}'?\nThis cannot be undone.",
                                "Delete", "Cancel"))
                        {
                            try
                            {
                                f.Delete();
                                if (i == _idx) { _idx = -1; _obj = null; _type = null; _fileName = null; }
                                Refresh();
                            }
                            catch (Exception e) { Debug.LogError("Delete failed: " + e); }
                        }
                        GUILayout.FlexibleSpace();
                    }
                }
            }
        }

        void DrawInspector()
        {
            EditorGUILayout.LabelField("Inspector", EditorStyles.boldLabel);

            if (_idx < 0 || _idx >= _files.Length)
            {
                EditorGUILayout.HelpBox("Select a file on the left to inspect.", MessageType.Info);
                return;
            }

            var fi = _files[_idx];

            if (_obj == null || _type == null)
            {
                EditorGUILayout.HelpBox("Not loaded yet or failed to load.\nClick 'Load'.", MessageType.Warning);
                InfoBox(fi);
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Type", _type.FullName);
                EditorGUILayout.LabelField("File", fi.FullName);
                EditorGUILayout.LabelField("Size", FmtBytes(fi.Length));
                EditorGUILayout.LabelField("Modified", fi.LastWriteTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            }

            EditorGUILayout.Space(4);
            try
            {
                EditorGUI.BeginChangeCheck();
                _obj = DrawAny(_type, _obj, _type.Name);
                if (EditorGUI.EndChangeCheck()) Repaint();
            }
            catch (Exception e)
            {
                EditorGUILayout.HelpBox("Render error: " + e, MessageType.Error);
            }

            EditorGUILayout.Space(8);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save", GUILayout.Width(120))) Save();
                if (GUILayout.Button("Reload", GUILayout.Width(120))) Load(_idx);
                GUILayout.FlexibleSpace();
            }
        }

        void InfoBox(FileInfo fi)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("File", fi.FullName);
                EditorGUILayout.LabelField("Size", FmtBytes(fi.Length));
                EditorGUILayout.LabelField("Modified", fi.LastWriteTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }

        void Load(int i)
        {
            _obj = null; _type = null; _fileName = null;
            if (i < 0 || i >= _files.Length) return;

            var fi = _files[i];
            _fileName = fi.Name; // hệ của bạn lưu tên file == typeof(T).ToString()

            _type = ResolveType(_fileName);
            if (_type == null)
            {
                EditorUtility.DisplayDialog("Type Not Found",
                    $"Cannot resolve type: {_fileName}\n\nEnsure class/namespace/assembly khớp khi save.",
                    "OK");
                return;
            }

            try
            {
                var bytes = File.ReadAllBytes(fi.FullName);
                _obj = SerializationUtility.DeserializeValueWeak(bytes, DataFormat.Binary)
                       ?? Activator.CreateInstance(_type);
            }
            catch (Exception e)
            {
                Debug.LogError("Load failed: " + e);
                EditorUtility.DisplayDialog("Load Failed", e.ToString(), "OK");
            }
        }

        void Save()
        {
            if (_obj == null || _type == null || string.IsNullOrEmpty(_fileName)) return;

            try
            {
                var path = Path.Combine(_root, _fileName);
                var bytes = SerializationUtility.SerializeValueWeak(_obj, DataFormat.Binary);
                File.WriteAllBytes(path, bytes);
                AssetDatabase.Refresh();
                Refresh();
                Repaint();
                EditorUtility.DisplayDialog("Saved", $"Saved to:\n{path}", "OK");
            }
            catch (Exception e)
            {
                Debug.LogError("Save failed: " + e);
                EditorUtility.DisplayDialog("Save Failed", e.ToString(), "OK");
            }
        }

        // ---------- Generic Drawer ----------
        object DrawAny(Type t, object v, string label)
        {
            // primitives
            if (t == typeof(string)) return EditorGUILayout.TextField(label, v as string ?? "");
            if (t == typeof(int))    return EditorGUILayout.IntField(label, v is int i ? i : 0);
            if (t == typeof(float))  return EditorGUILayout.FloatField(label, v is float f ? f : 0f);
            if (t == typeof(bool))   return EditorGUILayout.Toggle(label, v is bool b && b);
            if (t == typeof(long))   return ParseNum(label, v, x => long.TryParse(x, out var k) ? k : (v is long l ? l : 0L));
            if (t == typeof(double)) return ParseNum(label, v, x => double.TryParse(x, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : (v is double dv ? dv : 0.0));
            if (t.IsEnum)            return EditorGUILayout.EnumPopup(label, v as Enum ?? (Enum)Enum.GetValues(t).GetValue(0));

            // IList
            if (typeof(IList).IsAssignableFrom(t))
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

                    var list = v as IList;
                    if (list == null)
                    {
                        if (GUILayout.Button("Create List"))
                        {
                            v = t.IsArray ? Array.CreateInstance(t.GetElementType() ?? typeof(object), 0) : Activator.CreateInstance(t);
                            list = v as IList;
                        }
                        else return v;
                    }

                    if (t.IsArray)
                    {
                        EditorGUILayout.HelpBox("Array editing is read-only here. Use List<T> for editing.", MessageType.Info);
                        var et = t.GetElementType() ?? typeof(object);
                        var arr = (Array)list;
                        for (int i = 0; i < arr.Length; i++) DrawAny(et, arr.GetValue(i), $"[{i}]");
                        return v;
                    }

                    var elemType = t.IsGenericType ? t.GetGenericArguments()[0] : typeof(object);
                    int size = Mathf.Max(0, EditorGUILayout.IntField("Size", list.Count));
                    while (list.Count < size) list.Add(CreateDefault(elemType));
                    while (list.Count > size) list.RemoveAt(list.Count - 1);

                    for (int i = 0; i < list.Count; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            list[i] = DrawAny(elemType, list[i], $"[{i}]");
                            if (GUILayout.Button("X", GUILayout.Width(22))) { list.RemoveAt(i); i--; }
                        }
                    }
                    if (GUILayout.Button("+ Add")) list.Add(CreateDefault(elemType));
                    return list;
                }
            }

            // IDictionary
            if (typeof(IDictionary).IsAssignableFrom(t))
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

                    var dict = v as IDictionary;
                    if (dict == null)
                    {
                        if (GUILayout.Button("Create Dictionary")) { v = Activator.CreateInstance(t); dict = v as IDictionary; }
                        else return v;
                    }

                    Type keyT = typeof(object), valT = typeof(object);
                    if (t.IsGenericType) { var a = t.GetGenericArguments(); keyT = a[0]; valT = a[1]; }

                    var keys = new List<object>();
                    foreach (DictionaryEntry e in dict) keys.Add(e.Key);

                    int remove = -1;
                    for (int i = 0; i < keys.Count; i++)
                    {
                        var k = keys[i];
                        var oldV = dict[k];

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            var newK = DrawKey(keyT, k, $"Key [{i}]");
                            var newV = DrawAny(valT, oldV, $"Value [{i}]");

                            if (!Equals(newV, oldV)) dict[k] = newV;

                            if (!Equals(newK, k) && newK != null && !dict.Contains(newK))
                            { dict.Remove(k); dict[newK] = newV; keys[i] = newK; }

                            if (GUILayout.Button("X", GUILayout.Width(22))) remove = i;
                        }
                    }
                    if (remove >= 0) dict.Remove(keys[remove]);

                    if (GUILayout.Button("+ Add"))
                    {
                        var dk = DefaultKey(keyT, dict);
                        if (dk != null && !dict.Contains(dk)) dict[dk] = CreateDefault(valT);
                    }
                    return dict;
                }
            }

            // class/struct
            if (t.IsClass || t.IsValueType)
            {
                // Trường hợp đặc biệt: GOCDValue<T>
                if (IsGOCDValueType(t))
                    return DrawGOCDValue(t, v, label);

                if (v == null) v = CreateDefault(t);
                string key = label + "##" + t.FullName;
                var open = _fold.GetValueOrDefault(key, true);
                _fold[key] = EditorGUILayout.Foldout(open, $"{label} ({t.Name})", true);

                if (_fold[key])
                {
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

                        // Properties (public, có get/set, không indexer) — ƯU TIÊN PROPERTY
                        var props = t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                     .Where(pp => pp.CanRead && pp.CanWrite && pp.GetIndexParameters().Length == 0)
                                     .ToArray();
                        var propNames = new HashSet<string>(props.Select(p => NormalizeName(p.Name)));

                        foreach (var p in props)
                        {
                            try {
                                var pv = p.GetValue(v);
                                var nv = DrawAny(p.PropertyType, pv, p.Name);
                                if (!Equals(nv, pv)) p.SetValue(v, nv); // gọi setter
                            }
                            catch (Exception ex) {
                                EditorGUILayout.HelpBox($"Property '{p.Name}' error: {ex.Message}", MessageType.None);
                            }
                        }

                        // Fields (public hoặc [SerializeField]), bỏ qua nếu trùng tên property
                        foreach (var f in t.GetFields(flags)
                                           .Where(ff => (ff.IsPublic || ff.GetCustomAttribute<SerializeField>() != null)
                                                        && !ff.IsNotSerialized
                                                        && ff.GetCustomAttribute<HideInInspector>() == null))
                        {
                            if (propNames.Contains(NormalizeName(f.Name))) continue; // tránh trùng với property

                            try {
                                var fv = f.GetValue(v);
                                var nv = DrawAny(f.FieldType, fv, f.Name);
                                if (!Equals(nv, fv)) f.SetValue(v, nv);
                            }
                            catch (Exception ex) {
                                EditorGUILayout.HelpBox($"Field '{f.Name}' error: {ex.Message}", MessageType.None);
                            }
                        }
                    }
                }
                return v;
            }

            EditorGUILayout.LabelField(label, $"(Unsupported: {t.FullName})");
            return v;
        }

        // ---------- GOCDValue<T> drawer ----------
        object DrawGOCDValue(Type t, object v, string label)
        {
            // T
            var args = t.GetGenericArguments();
            var elemT = args.Length > 0 ? args[0] : typeof(object);

            // Tạo mặc định nếu null (GOCDValue<T>(default(T)))
            if (v == null)
            {
                try
                {
                    object dv = elemT.IsValueType ? Activator.CreateInstance(elemT) : null;
                    v = Activator.CreateInstance(t, new object[] { dv });
                }
                catch { return v; }
            }

            // Foldout
            string key = label + "##" + t.FullName;
            var open = _fold.GetValueOrDefault(key, true);
            _fold[key] = EditorGUILayout.Foldout(open, $"{label} (DataValue<{elemT.Name}>)", true);
            if (!_fold[key]) return v;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var prop = t.GetProperty("value", BindingFlags.Instance | BindingFlags.Public);
                var field = t.GetField("_value", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                // Header + toggle raw
                bool showRaw = _goRaw.TryGetValue(v, out var r) && r;
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Mode:", GUILayout.Width(45));
                    var btn = GUILayout.Toggle(showRaw, "Raw _value", "MiniButton", GUILayout.Width(90));
                    if (btn != showRaw) { _goRaw[v] = btn; showRaw = btn; }
                    GUILayout.FlexibleSpace();
                }

                // Property value (events)
                if (prop != null && prop.CanRead && prop.CanWrite && prop.GetIndexParameters().Length == 0)
                {
                    try {
                        var pv = prop.GetValue(v);
                        var nv = DrawAny(elemT, pv, "value");
                        if (!Equals(nv, pv)) prop.SetValue(v, nv); // gọi setter => chạy event
                    }
                    catch (Exception ex) { EditorGUILayout.HelpBox($"Property 'value' error: {ex.Message}", MessageType.None); }
                }
                else
                {
                    EditorGUILayout.HelpBox("Property 'value' not found or not writable.", MessageType.Warning);
                }

                // Raw field _value (no events)
                if (showRaw && field != null)
                {
                    try {
                        var fv = field.GetValue(v);
                        var nv = DrawAny(elemT, fv, "_value (raw)");
                        if (!Equals(nv, fv)) field.SetValue(v, nv); // set trực tiếp, không event
                    }
                    catch (Exception ex) { EditorGUILayout.HelpBox($"Field '_value' error: {ex.Message}", MessageType.None); }
                }
            }
            return v;
        }

        // ---------- Helpers ----------
        static string NormalizeName(string n)
        {
            if (string.IsNullOrEmpty(n)) return n;
            if (n.StartsWith("m_")) n = n.Substring(2);
            n = n.TrimStart('_');
            if (n.StartsWith("<") && n.Contains(">")) // backing field: <Name>k__BackingField
            {
                int i = n.IndexOf('>');
                if (i > 1) n = n.Substring(1, i - 1);
            }
            return n.ToLowerInvariant();
        }

        static bool IsGOCDValueType(Type t)
            => t.IsGenericType && t.GetGenericTypeDefinition().Name.StartsWith("DataValue`") && t.Namespace == "CodeSketch.Data";

        object ParseNum(string label, object cur, Func<string, object> parser)
        {
            var s = EditorGUILayout.TextField(label, cur?.ToString() ?? "0");
            return parser(s);
        }

        object DrawKey(Type kt, object cur, string label)
        {
            if (kt == typeof(string)) return EditorGUILayout.TextField(label, cur as string ?? "");
            if (kt == typeof(int))    return EditorGUILayout.IntField(label, cur is int i ? i : 0);
            if (kt == typeof(long))   return ParseNum(label, cur, x => long.TryParse(x, out var k) ? k : (cur is long l ? l : 0L));
            if (kt == typeof(float))  return EditorGUILayout.FloatField(label, cur is float f ? f : 0f);
            if (kt == typeof(double)) return ParseNum(label, cur, x => double.TryParse(x, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : (cur is double dv ? dv : 0.0));
            if (kt.IsEnum)            return EditorGUILayout.EnumPopup(label, cur as Enum ?? (Enum)Enum.GetValues(kt).GetValue(0));
            EditorGUILayout.LabelField(label, cur != null ? cur.ToString() : "(null)");
            return cur;
        }

        object DefaultKey(Type kt, IDictionary dict)
        {
            if (kt == typeof(string))
            {
                string b = "key", k = b; int i = 1; while (dict.Contains(k)) { k = b + i; i++; } return k;
            }
            if (kt == typeof(int))
            {
                int max = -1; foreach (DictionaryEntry e in dict) if (e.Key is int iv && iv > max) max = iv; return max + 1;
            }
            if (kt.IsEnum) return Enum.GetValues(kt).GetValue(0);
            if (kt == typeof(long)) return 0L;
            if (kt == typeof(float)) return 0f;
            if (kt == typeof(double)) return 0.0;
            try { return Activator.CreateInstance(kt); } catch { return null; }
        }

        static object CreateDefault(Type t)
        {
            if (t == typeof(string)) return "";
            try { return Activator.CreateInstance(t); }
            catch { return null; }
        }

        static string FmtBytes(long b)
        {
            string[] s = { "B", "KB", "MB", "GB" };
            double v = b; int o = 0; while (v >= 1024 && o < s.Length - 1) { o++; v /= 1024; }
            return $"{v:0.##} {s[o]}";
        }

        static Type ResolveType(string fullName)
        {
            var t = Type.GetType(fullName);
            if (t != null) return t;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                try { t = a.GetType(fullName); if (t != null) return t; } catch { }
            }
            return null;
        }
    }
}
#endif
