using GOCD.Framework.Diagnostics;
using UnityEngine;

namespace GOCD.Framework
{
    /// <summary>
    /// Singleton pattern implementation.
    /// Can be used with classes extended from a MonoBehaviour.
    /// Once instance is found or created, game object will be marked as DontDestroyOnLoad.
    /// </summary>
    public abstract class MonoSingleton<T> : MonoBase where T : MonoBehaviour
    {
        protected virtual bool _dontDestroyOnLoad => true;

        static T s_instance;
        static bool s_applicationIsQuitting = false;
        static bool s_isDestroyed = false;
        
        /// <summary>
        /// Returns a singleton class instance
        /// If current instance is not assigned it will try to find an object of the instance type,
        /// in case instance already exists on a scene. If not, new instance will be created
        /// </summary>
        public static T Instance
        {
            get
            {
                if (s_applicationIsQuitting || s_isDestroyed)
                {
                    GOCDDebug.LogError<T>(
                        $"{typeof(T)} is already destroyed. " +
                        $"Please check {nameof(hasInstance)} or {nameof(IsDestroyed)} before accessing instance in the destructor.", Color.cyan);
                    return null;
                }

                if (s_instance == null)
                {
                    s_instance = FindObjectOfType(typeof(T)) as T;
                    if (s_instance == null)
                        Instantiate();
                }

                return s_instance;
            }
        }
        
        /// <summary>
        /// Returns `true` if Singleton Instance exists.
        /// </summary>
        public static bool hasInstance => s_instance != null;
        
        /// <summary>
        /// If this property returns `true` it means that object with explicitly destroyed.
        /// This could happen if Destroy function  was called for this object or if it was
        /// automatically destroyed during the `ApplicationQuit`.
        /// </summary>
        public static bool IsDestroyed => s_isDestroyed;
        
        /// <summary>
        /// Methods will create new object Instantiate
        /// Normally method is called automatically when you referring to and Instance getter
        /// for a first time.
        /// But it may be useful if you want manually control when the instance is created,
        /// even if you do not this specific instance at the moment
        /// </summary>
         static void Instantiate()
        {
            if (hasInstance)
            {
                GOCDDebug.LogWarning<T>($"You are trying to Instantiate {typeof(T).FullName}, but it already has an instance. Please use instance property instead.", Color.cyan);
                return;
            }

            var name = typeof(T).FullName;
            s_instance = new GameObject(name).AddComponent<T>();
        }
        
        #region MonoBehaviour

        protected virtual void Awake()
        {
            if (s_instance == null)
            {
                s_isDestroyed = false;

                s_instance = this as T;

                if (_dontDestroyOnLoad)
                    DontDestroyOnLoad(GameObjectCached);
            }
            else if (s_instance != this)
            {
                GOCDDebug.Log<T>($"{typeof(T).FullName} is already exist, this one will be destroyed", Color.cyan);

                Destroy(GameObjectCached);
            }
        }

        /// <summary>
        /// When Unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it have been destroyed,
        /// it will create a buggy ghost object that will stay on the Editor scene
        /// even after stopping playing the Application. Really bad!
        /// So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (s_instance == this)
            {
                s_instance = null;
                s_isDestroyed = true;
            }
        }

        protected virtual void OnApplicationQuit()
        {
            s_instance = null;
            s_isDestroyed = true;
            s_applicationIsQuitting = true;
        }

        #endregion
    }
}
