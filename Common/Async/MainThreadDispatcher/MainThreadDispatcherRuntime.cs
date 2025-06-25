using UnityEngine;

namespace GOCD.Framework
{
    class MainThreadDispatcherRuntime : MainThreadDispatcherBase
    {
        public override void Init()
        {
            MonoCallback.Instance.EventUpdate += Update;
        }
    }
}
