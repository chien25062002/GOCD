namespace Game
{
    public class SettingSoundButton : SettingButton
    {
        protected override bool isOn { get { return DataSettings.SoundVolume.value > 0.0f; } set { DataSettings.SoundVolume.value = value ? 1.0f : 0.0f; } }
    }
}
