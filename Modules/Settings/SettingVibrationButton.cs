namespace Game
{
    public class SettingVibrationButton : SettingButton
    {
        protected override bool isOn
        {
            get => DataSettings.Vibration.value;
            set => DataSettings.Vibration.value = value;
        }
    }
}
