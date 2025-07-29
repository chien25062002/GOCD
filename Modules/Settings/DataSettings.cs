using GOCD.Framework;
using UnityEngine;

namespace Game
{
    public class DataSettings : GOCDDataBlock<DataSettings>
    {
        [SerializeField] GOCDValue<float> _soundVolume;
        [SerializeField] GOCDValue<float> _musicVolume;
        [SerializeField] GOCDValue<bool> _vibration;

        public static GOCDValue<float> SoundVolume => instance._soundVolume;
        public static GOCDValue<float> MusicVolume => instance._musicVolume;
        public static GOCDValue<bool> Vibration => instance._vibration;

        protected override void Init()
        {
            base.Init();

            _soundVolume = _soundVolume ?? new GOCDValue<float>(1.0f);
            _musicVolume = _musicVolume ?? new GOCDValue<float>(1.0f);
            _vibration = _vibration ?? new GOCDValue<bool>(true);
        }
    }
}
