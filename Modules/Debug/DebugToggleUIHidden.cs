using CodeSketch.Data;

namespace CodeSketch.Debug
{
    public class DebugToggleUIHidden : DebugToggle
    {
        protected override bool isOn 
        { 
            get => DataMaster.UIHidden.Value;
            set => DataMaster.UIHidden.Value = value;
        }
    }
}
