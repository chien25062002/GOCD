using System;

namespace GOCD.Framework
{
    interface IMainThreadDispatcher
    {
        void Init();
        void Enqueue(Action action);
    }
}
