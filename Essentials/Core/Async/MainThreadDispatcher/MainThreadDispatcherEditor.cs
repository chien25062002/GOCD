#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CodeSketch.Core.MainThread
{
    class MainThreadDispatcherEditor : MainThreadDispatcherBase
    {
        public override void Init()
        {
#if UNITY_EDITOR
            EditorApplication.update += Tick;
#else
            throw new System.NotSupportedException($"Attempted to run on  {UnityEngine.Application.platform}. Only editor platform is supported");
#endif
        }
    }
}
