using CodeSketch.Data;

namespace CodeSketch.Debug
{
    public class DebugToggleAdRewardSkip : DebugToggle
    {
        protected override bool isOn 
        {
            get => DataMaster.AdsRewardSkip.Value;
            set => DataMaster.AdsRewardSkip.Value = value;
        }
    }
}