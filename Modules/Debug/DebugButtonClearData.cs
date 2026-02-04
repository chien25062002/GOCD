using CodeSketch.Data;
using UnityEngine;

namespace CodeSketch.Debug
{
    public class DebugButtonClearData : DebugButton
    {
        public override void Button_OnClick()
        {
            base.Button_OnClick();

            DataFileHandler.DeleteAllInDevice();

            Application.Quit();
        }
    }
}
