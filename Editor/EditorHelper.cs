#if UNITY_EDITOR
using System.IO;
using GOCD.Framework.Audio;
using UnityEditor;
using UnityEngine;

namespace GOCD.Framework.Editor
{
    public static class EditorHelper
    {
        [MenuItem("CFramework/Clear Device Data")]
         static void ClearDeviceData()
        {
            CDataBlockHelper.ClearDeviceData();
        }

        [MenuItem("CFramework/Create Audio Config File")]
        public static void CreateAudioConfig()
        {
            foreach (var s in Selection.objects)
            {
                var assetPath = AssetDatabase.GetAssetPath(s);

                if (s.GetType() != typeof(AudioClip))
                    continue;

                AudioClip clip = (AudioClip)s;

                AudioConfig config = ScriptableObjectHelper.CreateAsset<AudioConfig>(Path.GetDirectoryName(assetPath), clip.name);

                if (config == null)
                    continue;

                config.Construct(clip);

                ScriptableObjectHelper.SaveAsset(config);
            }
        }
    }
}

#endif