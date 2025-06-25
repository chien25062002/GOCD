using UnityEngine;

namespace GOCD.Framework
{
    public class DebugButtonClearData : DebugButton
    {
        public override void Button_OnClick()
        {
            base.Button_OnClick();

            GOCDDataBlockHelper.DeleteAllInDevice();

            Application.Quit();
        }
    }
}
