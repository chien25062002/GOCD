using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace GOCD.Framework.Editor
{
    public class ScriptableObjectFactory
    {
        /// <summary>
        /// Helper class for instantiating ScriptableObjects.
        /// Class này hỗ trợ tìm kiếm các ScriptableObject trong project và tạo nhanh chúng
        /// </summary>
        ///
        [MenuItem("Assets/Create/Scriptable Object", false, 0)]
        public static void CreateAssembly()
        {
            List<string> assemblyNames = new List<string>();
            
            assemblyNames.Add("Assembly-CSharp");
            assemblyNames.Add("CFramework");
            assemblyNames.Add("CFramework.Runtime");
            assemblyNames.Add("CFramework.Module");
            
            string folderPath = Path.Combine(Application.dataPath, "_Game/"); // Đường dẫn tới thư mục Game
            assemblyNames.AddRange(LoadAssembliesFromFolder(folderPath));
            
            Create(assemblyNames.ToArray());
            //Create("Assembly-CSharp", "CFramework", "CFramework.Runtime", "CFramework.Module", "Game", "Game.FlowFree");
        }
        
        /// <summary>
        /// Tìm kiếm và trả về tên của các assembly trong thư mục chỉ định
        /// </summary>
        private static List<string> LoadAssembliesFromFolder(string folderPath)
        {
            var assemblyNames = new List<string>();
            
            if (Directory.Exists(folderPath))
            {
                var asmdefFiles = Directory.GetFiles(folderPath, "*.asmdef", SearchOption.AllDirectories);

                foreach (var asmdefFile in asmdefFiles)
                {
                    try
                    {
                        var jsonText = File.ReadAllText(asmdefFile);
                        var jsonObj = JObject.Parse(jsonText);
                        var assemblyName = jsonObj["name"].ToString();
                        assemblyNames.Add(assemblyName);
                    }
                    catch
                    {
                        // Bỏ qua nếu không thể đọc hoặc parse file asmdef
                    }
                }
            }

            return assemblyNames;
        }
        
        
        /// <summary>
        /// Search All ScriptableObject class available in the project and create new window to show all that ScriptableObjects
        /// Đưa vào tên các Assembly cần tìm kiếm code
        /// Tìm kiếm các script c# nằm trong các Assembly và kiểm tra xem chúng có phải ScriptableObject không
        /// Lấy toàn bộ ScriptableObject đó và hiển thị lên một cửa sổ mới để dễ dàng tạo mới chúng.
        /// </summary>
        /// <param name="assemblyNames"></param>
        public static void Create(params string[] assemblyNames)
        {
            var allScriptableObjects = new List<System.Type>();

            foreach (string assemblyName in assemblyNames)
            {
                var assembly = GetAssembly(assemblyName);

                if (assembly == null)
                    continue;

                allScriptableObjects.AddRange((from t in assembly.GetTypes()
                    where t.IsSubclassOf(typeof(ScriptableObject))
                    select t).ToArray());
            }

            // Show the selection window.
            var window = EditorWindow.GetWindow<ScriptableObjectWindow>(true, "Create a new ScriptableObject", true);
            window.ShowPopup();

            window.Types = allScriptableObjects.ToArray();
        }
        
        /// <summary>
        /// Returns the assembly that contains the script code for this project (currently hard coded)
        /// Trả về assembly chứa code với tên chỉ định
        /// </summary>
        private static Assembly GetAssembly(string name)
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
