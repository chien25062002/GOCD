#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json.Linq;

[InitializeOnLoad]
public static class GOCD_AutoInstallDependencies
{
    static GOCD_AutoInstallDependencies()
    {
        try
        {
            string manifestPath = Path.Combine(Application.dataPath, "../Packages/manifest.json");
            if (!File.Exists(manifestPath))
            {
                Debug.LogWarning("[GOCD] manifest.json not found!");
                return;
            }

            string json = File.ReadAllText(manifestPath);
            var manifest = JObject.Parse(json);
            var deps = manifest["dependencies"] as JObject;

            const string packageKey = "com.primetween.core";
            const string packageUrl = "https://github.com/KyryloKuzyk/PrimeTween.git";

            if (deps[packageKey] == null)
            {
                deps[packageKey] = packageUrl;
                File.WriteAllText(manifestPath, manifest.ToString());
                Debug.Log("[GOCD] PrimeTween dependency added to manifest.json. Unity will now reimport packages.");
                AssetDatabase.Refresh();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[GOCD] Failed to auto-add PrimeTween:\n" + ex);
        }
    }
}
#endif
