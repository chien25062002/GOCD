using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

using CodeSketch.SO;

namespace CodeSketch.Core
{
    public class CodeSketchFactory : ScriptableObjectSingleton<CodeSketchFactory>
    {
        [Title("Prefabs")]
        [SerializeField] GameObject _UINotificationText;
        [SerializeField] GameObject _popupDebug;

        
        [Title("View")]
        [SerializeField] AssetReference _internetCheck;
        
        public static GameObject UINotificationText => Instance._UINotificationText;
        public static GameObject PopupDebug => Instance._popupDebug;
        
        public static AssetReference InternetCheck => Instance._internetCheck;
    }
}
