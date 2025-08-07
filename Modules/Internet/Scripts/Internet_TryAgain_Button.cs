using GOCD.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace GOCD.Framework.Internet
{
    public class Internet_TryAgain_Button : UIButtonBase
    {
        [SerializeField] Image _imgFrame;
        [SerializeField] Material _grayscaleMat;

        Internet_View _internetView;

        public Internet_View InternetView
        {
            get
            {
                if (_internetView == null)
                    _internetView = GetComponentInParent<Internet_View>();
                return _internetView;
            }
        }

        public override void Button_OnClick()
        {
            base.Button_OnClick();
            
            SetInteractable(false);
            InternetView.TryConnect();
        }

        public void SetInteractable(bool isInteractable)
        {
            button.interactable = isInteractable;
            _imgFrame.material = isInteractable ? null : _grayscaleMat;
        }
    }
}
