using UnityEngine.UI;

namespace GOCD.Framework
{
    public class UIButtonBase : MonoCached
    {
        Button _button;

        public Button button
        {
            get
            {
                if (_button == null)
                    _button = GetComponent<Button>();
                
                if(_button == null)
                    _button = GetComponentInChildren<Button>(false);

                return _button;
            }
        }

        protected virtual void Awake()
        {
            button.onClick.AddListener(Button_OnClick);
        }

        public virtual void Button_OnClick()
        {
        }
    }
}