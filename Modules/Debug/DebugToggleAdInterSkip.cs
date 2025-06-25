using GOCD.Framework.Data;

namespace GOCD.Framework
{
    public class DebugToggleAdInterSkip : DebugToggle
    {
        protected override bool isOn
        { 
            get => DataMaster.AdsInterSkip.value;
            set => DataMaster.AdsInterSkip.value = value;
        }
    }
}
