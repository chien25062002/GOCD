using TMPro;
using UnityEngine;

namespace GOCD.Framework
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UITextBase : MonoBase
    {
        TextMeshProUGUI _text;

        public TextMeshProUGUI text
        {
            get
            {
                if (_text == null)
                    _text = GetComponent<TextMeshProUGUI>();

                return _text;
            }
        }

        protected virtual void Awake()
        {
            _text = text;
        }
    }
}
