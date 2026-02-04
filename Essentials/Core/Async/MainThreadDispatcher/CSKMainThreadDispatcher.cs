using System;
using UnityEngine;

namespace CodeSketch.Core.MainThread
{
    /// <summary>
    /// Unity API isn't thread safe, so in case you are using threads,
    /// and you need to call any Unity API from that thread, this class is exactly what you need.
    /// The <see cref="CSKMainThreadDispatcher"/> is available for Editor and Play mode usage.
    /// </summary>
    public static class CSKMainThreadDispatcher
    {
        static IMainThreadDispatcher s_MainThreadDispatcher;

        static void LazyInit()
        {
            if (s_MainThreadDispatcher != null)
                return;

            if (Application.isEditor)
                s_MainThreadDispatcher = new CSKMainThreadDispatcherEditor();
            else
                s_MainThreadDispatcher = new CSKMainThreadDispatcherRuntime();

            s_MainThreadDispatcher.Init();
        }
        
        /// <summary>
        /// Adds an <see cref="Action"/> to the main thread queue.
        /// The Action will be dispatched under a main thread on a next frame.
        /// </summary>
        /// <param name="action">The callback action.</param>
        public static void Enqueue(Action action)
        {
            LazyInit();
            s_MainThreadDispatcher.Enqueue(action);
        }
    }
}
