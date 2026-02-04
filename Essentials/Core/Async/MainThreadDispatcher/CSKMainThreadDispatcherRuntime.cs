using CodeSketch.Mono;
using UnityEngine;

namespace CodeSketch.Core.MainThread
{
    class CSKMainThreadDispatcherRuntime : CSKMainThreadDispatcherBase
    {
        public override void Init()
        {
            MonoCallback.SafeInstance.EventUpdate += Tick;
        }
    }
}
