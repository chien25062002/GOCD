using GOCD.Framework.Data;

namespace GOCD.Framework
{
    public class DebugToggleUIHidden : DebugToggle
    {
        protected override bool isOn 
        { 
            get => DataMaster.UiHidden.value;
            set => DataMaster.UiHidden.value = value;
        }
    }
}
