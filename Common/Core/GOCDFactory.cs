using GOCD.Framework.Audio;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GOCD.Framework
{
    public class GOCDFactory : ScriptableObjectSingleton<GOCDFactory>
    {
        [Title("Scene Loader")] 
        [SerializeField] float _sceneLoaderFadeInDuration = 0.2f;
        [SerializeField] float _sceneLoaderLoadDuration = 0.1f;
        [SerializeField] float _sceneLoaderFadeOutDuration = 0.2f;
        [SerializeField] GameObject _sceneLoaderPrefab;

        [Title("Pools")] 
        [SerializeField] PoolPrefabConfig _poolTextDamage;

        [Space]
        
        [Title("Prefabs")]
        [SerializeField] GameObject _uiNotificationText;
        [SerializeField] GameObject _debugPopup;

        [Space] 
        
        [Title("UI")] 
        [SerializeField] AudioConfig _sfxUIButtonClick;
        
        [Title("View")]
        [SerializeField] AssetReference _viewAdsBreak;
        [SerializeField] AssetReference _internetCheck;
        
        public static float sceneTransitionFadeInDuration => Instance._sceneLoaderFadeInDuration;
        public static float sceneTransitionLoadDuration => Instance._sceneLoaderLoadDuration;
        public static float sceneTransitionFadeOutDuration => Instance._sceneLoaderFadeOutDuration;
        public static GameObject sceneTransitionPrefab => Instance._sceneLoaderPrefab;
        
        public static PoolPrefabConfig poolTextDamage => Instance._poolTextDamage;

        public static GameObject UINotificationText => Instance._uiNotificationText;
        public static GameObject DebugPopup => Instance._debugPopup;
        public static AssetReference InternetCheck => Instance._internetCheck;

        public static AudioConfig SfxUIButtonClick => Instance._sfxUIButtonClick;
        
        public static AssetReference ViewAdsBreak => Instance._viewAdsBreak;
    }
}
