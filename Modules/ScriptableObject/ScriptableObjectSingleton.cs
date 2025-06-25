using GOCD.Framework.Editor;
using UnityEngine;

namespace GOCD.Framework
{
    public abstract class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        static string s_rootFolderName => "ScriptableObjectSingletons";
        
        static T s_instance = null;
        
        public static T Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = Resources.Load<T>(string.Format("{0}/{1}", s_rootFolderName, typeof(T)));

#if UNITY_EDITOR
                    if (s_instance == null)
                    {
                        string configPath = string.Format("Assets/Resources/{0}/", s_rootFolderName);

                        if (!System.IO.Directory.Exists(configPath))
                            System.IO.Directory.CreateDirectory(configPath);

                        s_instance = ScriptableObjectHelper.CreateAsset<T>(configPath, typeof(T).ToString());
                    }
#endif
                    (s_instance as ScriptableObjectSingleton<T>)?.OnInitialize();
                }

                return s_instance;
            }
        }

        public static void Release()
        {
            (Instance as ScriptableObjectSingleton<T>)?.OnRelease();
            s_instance = null;
            Resources.UnloadUnusedAssets();
        }

        protected virtual void OnInitialize()
        {
            
        }

        protected virtual void OnRelease()
        {
            
        }
    }
}
