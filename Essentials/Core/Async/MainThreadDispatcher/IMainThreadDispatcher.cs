using System;

namespace CodeSketch.Core.MainThread
{
    interface IMainThreadDispatcher
    {
        void Init();
        void Enqueue(Action action);
    }
}
