using GOCD.Framework.Data;

namespace GOCD.Framework
{
    public class DebugToggleAdRewardSkip : DebugToggle
    {
        protected override bool isOn 
        {
            get => DataMaster.AdsRewardSkip.value;
            set => DataMaster.AdsRewardSkip.value = value;
        }
    }
}