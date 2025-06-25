using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GOCD.Framework
{
    /// <summary>
    /// The MonoCallback helper class is meant to be used when you need to have MonoBehaviour default Unity callbacks,
    /// but your model is not a MonoBehaviour and you dont want to convert it to the MonoBehaviour by design.
    ///
    /// Please note, that you will subscribe to the global MonoBehaviour singleton instance. Other parts of code may also use it.
    /// In case other callback users will throw and unhandled exception you may not received the callback you subscribed for.
    
    /// <summary>
    /// Lớp trợ giúp MonoCallback được sử dụng khi bạn cần các callback mặc định của MonoBehaviour trong Unity,
    /// nhưng mô hình của bạn không phải là MonoBehaviour và bạn không muốn chuyển đổi nó thành MonoBehaviour theo thiết kế.
    ///
    /// Xin lưu ý rằng bạn sẽ đăng ký vào instance singleton toàn cục của MonoBehaviour. Các phần khác của mã cũng có thể sử dụng nó.
    /// Trong trường hợp các người dùng callback khác gây ra ngoại lệ không được xử lý, bạn có thể không nhận được callback mà bạn đã đăng ký.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class MonoCallback : MonoSingleton<MonoCallback>
    {
        /// <summary>
        /// Update is called every frame.
        /// Learn more: [MonoBehaviour.Update](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html)
        /// </summary>
        public event Action EventUpdate;
        
        /// <summary>
        /// LateUpdate is called after all Update functions have been called. This is useful to order script execution.
        /// For example a follow camera should always be implemented in LateUpdate because it tracks objects that might have moved inside Update.
        /// Learn more: [MonoBehaviour.LateUpdate](https://docs.unity3d.com/ScriptReference/MonoBehaviour.LateUpdate.html)
        /// </summary>
        public event Action EventLateUpdate;
        
        /// <summary>
        /// In the editor this is called when the user stops playmode.
        /// Learn more: [MonoBehaviour.OnApplicationQuit](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationQuit.html)
        /// </summary>
        public event Action EventApplicationQuit;
        
        /// <summary>
        /// Sent to all GameObjects when the application pauses.
        /// Learn more: [MonoBehaviour.OnApplicationPause](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationPause.html)
        /// </summary>
        public event Action<bool> EventApplicationPause;
        
        /// <summary>
        /// Sent to all GameObjects when the player gets or loses focus.
        /// Learn more: [MonoBehaviour.OnApplicationFocus](https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationFocus.html)
        /// </summary>
        public event Action<bool> EventApplicationFocus;
        
        /// <summary>
        /// Frame-rate independent MonoBehaviour.FixedUpdate message for physics calculations.
        /// Learn more: [MonoBehaviour.FixedUpdate](https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html)
        /// </summary>
        public event Action EventFixedUpdate;
        
        /// <summary>
        /// Called when active scene changed.
        /// </summary>
        public event Action<Scene, Scene> EventActiveSceneChanged;
        
        void Update() => EventUpdate?.Invoke();
        void LateUpdate() => EventLateUpdate?.Invoke();
        void FixedUpdate() => EventFixedUpdate?.Invoke();
        void OnApplicationPause(bool pauseStatus) => EventApplicationPause?.Invoke(pauseStatus);
        void OnApplicationFocus(bool hasFocus) => EventApplicationFocus?.Invoke(hasFocus);
        
        protected override void Awake()
        {
            base.Awake();

            SceneManager.activeSceneChanged += SceneManager_ActiveSceneChanged;
        }

        protected override void OnDestroy()
        {
            SceneManager.activeSceneChanged -= SceneManager_ActiveSceneChanged;
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            EventApplicationQuit?.Invoke();
        }

        void SceneManager_ActiveSceneChanged(Scene scenePrevious, Scene sceneCurrent)
        {
            EventActiveSceneChanged?.Invoke(scenePrevious, sceneCurrent);
        }

        protected virtual void OnEventActiveSceneChanged(Scene arg1, Scene arg2)
        {
            EventActiveSceneChanged?.Invoke(arg1, arg2);
        }
    }
}
