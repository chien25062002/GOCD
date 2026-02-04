using CodeSketch.Data;
using UnityEngine;

namespace CodeSketch.Settings
{
    public class DataSettings : DataBlock<DataSettings>
    {
        [SerializeField] DataValue<float> _soundVolume;
        [SerializeField] DataValue<float> _musicVolume;
        [SerializeField] DataValue<bool> _vibration;

        public static DataValue<float> SoundVolume => Instance._soundVolume;
        public static DataValue<float> MusicVolume => Instance._musicVolume;
        public static DataValue<bool> Vibration => Instance._vibration;

        protected override void Init()
        {
            base.Init();

            _soundVolume ??= new DataValue<float>(1.0f);
            _musicVolume ??= new DataValue<float>(1.0f);
            _vibration ??= new DataValue<bool>(true);
        }
    }
}
