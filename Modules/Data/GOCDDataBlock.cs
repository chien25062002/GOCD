using System;
using System.IO;

namespace GOCD.Framework
{
    public class GOCDDataBlock<T> where T : GOCDDataBlock<T>
    {
        static T s_instance;

        public static T instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = GOCDDataBlockHelper.LoadFromDevice<T>(typeof(T).ToString());

                    if (s_instance == null)
                        s_instance = (T)Activator.CreateInstance(typeof(T));

                    s_instance.Init();
                }

                return s_instance;
            }
        }

        protected virtual void Init()
        {
            MonoCallback.Instance.EventApplicationPause += MonoCallback_ApplicationOnPause;
            MonoCallback.Instance.EventApplicationQuit += MonoCallback_ApplicationOnQuit;

            CDataBlockHelper.eventDelete += LDataBlockHelper_EventDelete;
        }

        private void MonoCallback_ApplicationOnQuit()
        {
            Save();
        }

        private void MonoCallback_ApplicationOnPause(bool paused)
        {
            if (paused)
                Save();
        }

        private void LDataBlockHelper_EventDelete()
        {
            s_instance = null;
        }

        public static void Save()
        {
            GOCDDataBlockHelper.SaveToDevice(instance, typeof(T).ToString());
        }

        public static void Delete()
        {
            s_instance = null;

            GOCDDataBlockHelper.DeleteInDevice(typeof(T).ToString());
        }
    }

    public class CDataBlockHelper
    {
        public static event Action eventDelete;

        public static void ClearDeviceData()
        {
            eventDelete?.Invoke();

            GOCDDataBlockHelper.DeleteAllInDevice();
        }
    }
}