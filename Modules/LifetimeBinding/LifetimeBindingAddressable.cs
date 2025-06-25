using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using ArgumentNullException = System.ArgumentNullException;

namespace GOCD.Framework.LifetimeBinding
{
    /// <summary>
    /// Cung cấp các extension method để gắn vòng đời của Addressables vào GameObject hoặc LifetimeBinding
    /// </summary>
    public static class LifetimeBindingAddressable
    {
        /// <summary>
        /// Gắn vòng đời của AsyncOperationHandle (scene hoặc asset) vào GameObject.
        /// Nếu GameObject bị huỷ thì handle cũng sẽ được giải phóng.
        /// </summary>
        public static AsyncOperationHandle BindTo(this AsyncOperationHandle self, GameObject gameObject, bool isScene)
        {
            if (gameObject == null)
            {
                ReleaseHandle(self, isScene);
                throw new ArgumentNullException(nameof(gameObject),
                    "GameObject null, handle sẽ được giải phóng ngay lập tức.");
            }

            if (!gameObject.TryGetComponent(out LifetimeBinding lifetimeBinding))
                lifetimeBinding = gameObject.AddComponent<LifetimeBinding>();

            return self.BindTo(lifetimeBinding, isScene);
        }

        /// <summary>
        /// Gắn vòng đời của AsyncOperationHandle<T> vào GameObject.
        /// Tự động xác định nếu là SceneInstance.
        /// </summary>
        public static AsyncOperationHandle<T> BindTo<T>(this AsyncOperationHandle<T> self, GameObject gameObject)
        {
            bool isScene = typeof(T) == typeof(SceneInstance);

            if (gameObject == null)
            {
                ReleaseHandle(self, isScene);
                throw new ArgumentNullException(nameof(gameObject),
                    "GameObject null, handle sẽ được giải phóng ngay lập tức.");
            }

            ((AsyncOperationHandle)self).BindTo(gameObject, isScene);
            return self;
        }

        /// <summary>
        /// Gắn handle vào LifetimeBinding. Khi bị huỷ, handle sẽ tự động được release.
        /// </summary>
        public static AsyncOperationHandle BindTo(this AsyncOperationHandle self, LifetimeBinding lifetimeBinding, bool isScene)
        {
            if (lifetimeBinding == null)
            {
                ReleaseHandle(self, isScene);
                throw new ArgumentNullException(nameof(lifetimeBinding),
                    "LifetimeBinding null, handle sẽ được giải phóng ngay lập tức.");
            }

            void OnRelease()
            {
                ReleaseHandle(self, isScene);
                lifetimeBinding.EventRelease -= OnRelease;
            }

            lifetimeBinding.EventRelease += OnRelease;
            return self;
        }

        /// <summary>
        /// Gắn handle dạng T vào LifetimeBinding. Tự xác định nếu là scene.
        /// </summary>
        public static AsyncOperationHandle<T> BindTo<T>(this AsyncOperationHandle<T> self, LifetimeBinding lifetimeBinding)
        {
            bool isScene = typeof(T) == typeof(SceneInstance);

            if (lifetimeBinding == null)
            {
                ReleaseHandle(self, isScene);
                throw new ArgumentNullException(nameof(lifetimeBinding),
                    "LifetimeBinding null, handle sẽ được giải phóng ngay lập tức.");
            }

            ((AsyncOperationHandle)self).BindTo(lifetimeBinding, isScene);
            return self;
        }

        /// <summary>
        /// Gắn AssetReference (dạng Addressables prefab) vào GameObject để giải phóng khi bị huỷ.
        /// </summary>
        public static AssetReference BindTo(this AssetReference self, GameObject gameObject)
        {
            if (gameObject == null)
            {
                self.ReleaseAsset();
                throw new ArgumentNullException(nameof(gameObject),
                    "GameObject null, asset sẽ được release ngay lập tức.");
            }

            if (!gameObject.TryGetComponent(out LifetimeBinding lifetimeBinding))
                lifetimeBinding = gameObject.AddComponent<LifetimeBinding>();

            void OnRelease()
            {
                self.ReleaseInstance(lifetimeBinding.gameObject);
                lifetimeBinding.EventRelease -= OnRelease;
            }

            lifetimeBinding.EventRelease += OnRelease;
            return self;
        }

        /// <summary>
        /// Giải phóng handle đúng cách theo loại (scene hay asset).
        /// </summary>
        static void ReleaseHandle(AsyncOperationHandle handle, bool isScene)
        {
            if (isScene)
                Addressables.UnloadSceneAsync(handle);
            else
                Addressables.Release(handle);
        }
    }
}
