using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CodeSketch.Editor.Scriptable
{
    /// <summary>
    /// Helper class for instantiating ScriptableObjects.
    /// Class này hỗ trợ tìm kiếm các ScriptableObject trong project và tạo nhanh chúng
    /// </summary>
    public class ScriptableObjectLookup
    {
        // =====================================================
        // MENU
        // =====================================================

        [MenuItem("Assets/Create/Scriptable Object", false, 0)]
        public static void CreateAssembly()
        {
            List<string> assemblyNames = new List<string>
            {
                "Assembly-CSharp"
            };

            // Các subfolder muốn bỏ qua khi scan asmdef
            string[] ignoreSubFolders =
            {
                "CodeSketch/Editor",
                "CodeSketch/Modules",
                "CodeSketch/Essentials/Core/ScriptableObject/",
                "Plugins/OdinSerializer",
                "Plugins/StompyRobot"
            };

            LoadFromFolder(ref assemblyNames, "_Game/", ignoreSubFolders);
            LoadFromFolder(ref assemblyNames, "CodeSketch", ignoreSubFolders);

            Create(assemblyNames.ToArray());
        }

        // =====================================================
        // LOAD ASSEMBLY FROM FOLDER
        // =====================================================

        static void LoadFromFolder(
            ref List<string> assemblyNames,
            string folder,
            string[] ignoreSubFolders)
        {
            string folderPath = Path.Combine(Application.dataPath, folder);
            assemblyNames.AddRange(
                LoadAssembliesFromFolder(folderPath, ignoreSubFolders)
            );
        }

        /// <summary>
        /// Tìm kiếm và trả về tên của các assembly (.asmdef) trong thư mục chỉ định
        /// Có hỗ trợ ignore theo subfolder
        /// </summary>
        static List<string> LoadAssembliesFromFolder(
            string folderPath,
            string[] ignoreSubFolders)
        {
            var assemblyNames = new List<string>();

            if (!Directory.Exists(folderPath))
                return assemblyNames;

            var asmdefFiles = Directory.GetFiles(
                folderPath,
                "*.asmdef",
                SearchOption.AllDirectories
            );

            foreach (var asmdefFile in asmdefFiles)
            {
                // Normalize path cho an toàn cross-platform
                string normalizedPath = asmdefFile.Replace('\\', '/');

                // Ignore subfolders
                if (ignoreSubFolders != null && ignoreSubFolders.Length > 0)
                {
                    bool ignored = false;

                    for (int i = 0; i < ignoreSubFolders.Length; i++)
                    {
                        var ignore = ignoreSubFolders[i];
                        if (string.IsNullOrEmpty(ignore))
                            continue;

                        if (normalizedPath.Contains($"/{ignore}/"))
                        {
                            ignored = true;
                            break;
                        }
                    }

                    if (ignored)
                        continue;
                }

                try
                {
                    var jsonText = File.ReadAllText(asmdefFile);
                    var jsonObj = JObject.Parse(jsonText);
                    var assemblyName = jsonObj["name"].ToString();

                    if (!string.IsNullOrEmpty(assemblyName))
                        assemblyNames.Add(assemblyName);
                }
                catch
                {
                    // Ignore invalid or unreadable asmdef
                }
            }

            return assemblyNames;
        }

        // =====================================================
        // CREATE SCRIPTABLE OBJECT WINDOW
        // =====================================================

        /// <summary>
        /// Search all ScriptableObject classes in the given assemblies
        /// and show a popup window to create them.
        /// </summary>
        public static void Create(params string[] assemblyNames)
        {
            var allScriptableObjects = new List<System.Type>();

            foreach (string assemblyName in assemblyNames)
            {
                var assembly = GetAssembly(assemblyName);
                if (assembly == null)
                    continue;

                try
                {
                    allScriptableObjects.AddRange(
                        assembly.GetTypes()
                            .Where(t =>
                                t.IsSubclassOf(typeof(ScriptableObject)) &&
                                !t.IsAbstract)
                            .ToArray()
                    );
                }
                catch
                {
                    // Ignore broken assemblies
                }
            }

            if (allScriptableObjects.Count == 0)
                return;

            var window = EditorWindow.GetWindow<ScriptableObjectWindow>(
                true,
                "Create a new ScriptableObject",
                true
            );

            window.Types = allScriptableObjects.ToArray();
            window.ShowPopup();
        }

        // =====================================================
        // ASSEMBLY RESOLUTION
        // =====================================================

        /// <summary>
        /// Trả về assembly theo tên
        /// </summary>
        static Assembly GetAssembly(string name)
        {
            try
            {
                return Assembly.Load(new AssemblyName(name));
            }
            catch
            {
                return null;
            }
        }
    }
}
