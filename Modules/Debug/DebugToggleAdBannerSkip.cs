using CodeSketch.Data;

namespace CodeSketch.Debug
{
    public class DebugToggleAdBannerSkip : DebugToggle
    {
        protected override bool isOn 
        { 
            get => DataMaster.AdsBannerSkip.Value;
            set => DataMaster.AdsBannerSkip.Value = value;
        }
    }
}
