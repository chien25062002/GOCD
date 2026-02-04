using CodeSketch.Audio;
using CodeSketch.Core;
using CodeSketch.Settings;
using CodeSketch.UIPopup;
using UnityEngine;

namespace CodeSketch.Preset
{
    public static class CodeSketchMasterInit
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
            AudioManager.VolumnMusic.Value = DataSettings.MusicVolume.Value;
            AudioManager.VolumeSound.Value = DataSettings.SoundVolume.Value;

            DataSettings.MusicVolume.OnValueChanged += (volume) => { AudioManager.VolumnMusic.Value = volume; };
            DataSettings.SoundVolume.OnValueChanged += (volume) => { AudioManager.VolumeSound.Value = volume; };

            Haptic.Haptic.Enabled = DataSettings.Vibration.Value;
            //Taptic.tapticOn = DataSettings.Vibration.Value;

            DataSettings.Vibration.OnValueChanged += SettingsVibrationValue_EventValueChanged;
        }
        
        static void SettingsVibrationValue_EventValueChanged(bool isOn)
        {
            Haptic.Haptic.Enabled = DataSettings.Vibration.Value;
        }
        
        #endregion
        
        #region SRDebug

        static void InitSRDebug()
        {
            SRDebug.Init();
            SRDebug.Instance.PanelVisibilityChanged += (isVisible) => { if (!isVisible && CodeSketchFactory.PopupDebug != null) PopupManager.Create(CodeSketchFactory.PopupDebug); };
        }

        #endregion
    }
}
