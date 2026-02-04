using UnityEngine;

namespace CodeSketch.Debug
{
    public class DebugToggle : DebugButton
    {
        protected virtual bool isOn { get; set; }

        protected override void Awake()
        {
            base.Awake();

            UpdateUI();
        }

        public override void Button_OnClick()
        {
            base.Button_OnClick();

            isOn = !isOn;

            UpdateUI();
        }

        void UpdateUI()
        {
            Button.image.color = isOn ? Color.green : Color.red;
        }
    }
}
