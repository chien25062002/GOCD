using GOCD.Framework;
using UnityEngine;

namespace Game
{
    public class SettingButton : UIButtonBase
    {
        [SerializeField] GameObject _goEnable;
        [SerializeField] GameObject _goDisable;

        protected virtual bool isOn { get; set; }

        void Start()
        {
            _goEnable.SetActive(isOn);
            _goDisable.SetActive(!isOn);
        }

        public override void Button_OnClick()
        {
            base.Button_OnClick();
            
            isOn = !isOn;
            
            _goEnable.SetActive(isOn);
            _goDisable.SetActive(!isOn);
        }
    }
}
