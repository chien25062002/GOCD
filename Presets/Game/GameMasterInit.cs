using GOCD.Framework;
using GOCD.Framework.Audio;
using UnityEngine;

namespace Game
{
    public static class GameMasterInit
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void RuntimeInit()
        {
            if (Application.isMobilePlatform)
            {
                Application.targetFrameRate = Mathf.Min(Screen.currentResolution.refreshRate, 60);
                QualitySettings.vSyncCount = 0;
            }

            InitSettings();
            InitSRDebug();
        }
        
        #region Vibration Setting
        
        static void InitSettings()
        {
            AudioManager.volumeMusic.value = DataSettings.MusicVolume.value;
            AudioManager.volumeSound.value = DataSettings.SoundVolume.value;

            DataSettings.MusicVolume.OnValueChanged += (volume) => { AudioManager.volumeMusic.value = volume; };
            DataSettings.SoundVolume.OnValueChanged += (volume) => { AudioManager.volumeSound.value = volume; };

            Taptic.Taptic.tapticOn = DataSettings.Vibration.value;

            DataSettings.Vibration.OnValueChanged += SettingsVibrationValue_EventValueChanged;
        }
        
        static void SettingsVibrationValue_EventValueChanged(bool isOn)
        {
            Taptic.Taptic.tapticOn = isOn;
        }
        
        #endregion
        
        #region SRDebug

        static void InitSRDebug()
        {
            SRDebug.Init();
            SRDebug.Instance.PanelVisibilityChanged += (isVisible) => { if (!isVisible && GOCDFactory.DebugPopup != null) PopupManager.Create(GOCDFactory.DebugPopup); };
        }

        #endregion
    }
}
