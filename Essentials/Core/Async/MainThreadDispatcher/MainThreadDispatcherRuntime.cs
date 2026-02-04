using CodeSketch.Mono;
using UnityEngine;

namespace CodeSketch.Core.MainThread
{
    class MainThreadDispatcherRuntime : MainThreadDispatcherBase
    {
        public override void Init()
        {
            MonoCallback.SafeInstance.EventUpdate += Tick;
        }
    }
}
