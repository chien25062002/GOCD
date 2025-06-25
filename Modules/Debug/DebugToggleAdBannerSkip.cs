using GOCD.Framework.Data;

namespace GOCD.Framework
{
    public class DebugToggleAdBannerSkip : DebugToggle
    {
        protected override bool isOn 
        { 
            get => DataMaster.AdsBannerSkip.value;
            set => DataMaster.AdsBannerSkip.value = value;
        }
    }
}
