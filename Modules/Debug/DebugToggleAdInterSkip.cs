using CodeSketch.Data;

namespace CodeSketch.Debug
{
    public class DebugToggleAdInterSkip : DebugToggle
    {
        protected override bool isOn
        { 
            get => DataMaster.AdsInterSkip.Value;
            set => DataMaster.AdsInterSkip.Value = value;
        }
    }
}
