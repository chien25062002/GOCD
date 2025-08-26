using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GOCD.Framework.Internet
{
    public class InternetSettings : ScriptableObject
    {
        [Header("Check")]
        [Tooltip("URL để ping (HTTP GET).")]
        public string testUrl = "https://www.google.com";

        [Min(1)]
        [Tooltip("Timeout (giây) cho mỗi request.")]
        public int requestTimeout = 5;

        [Min(1f)]
        [Tooltip("Chu kỳ kiểm tra (giây) – dùng cho Internet_Checker loop.")]
        public float checkInterval = 10f;

        [Header("View Provider")]
        [Tooltip("Prefab chứa component Popup (ưu tiên nếu kéo thả prefab).")]
        public GameObject internetViewPrefab;

        [Tooltip("Addressable View (AssetReference GameObject) – dùng khi KHÔNG gán prefab.")]
        public AssetReferenceGameObject addressableView;

        // (Legacy) Nếu bạn vẫn đang dùng hệ GUID riêng thì có thể giữ, còn không thì bỏ.
        [Tooltip("(Legacy) Factory GUID cho hệ ViewHelper.PushAsync(guid).")]
        public string factoryGuid;

        public const string DefaultResourcesPath = "GOCD/InternetSettings";
    }
}