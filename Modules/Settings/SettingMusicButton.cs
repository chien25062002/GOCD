namespace Game
{
    public class SettingMusicButton : SettingButton
    {
        protected override bool isOn { get { return DataSettings.MusicVolume.value > 0.0f; } set { DataSettings.MusicVolume.value = value ? 1.0f : 0.0f; } }
    }
}
