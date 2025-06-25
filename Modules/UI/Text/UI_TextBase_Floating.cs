using TMPro;
using UnityEngine;

namespace GOCD.Framework
{
    public class UI_TextBase_Floating : MonoCached
    {
        [SerializeField, HideInInspector] TextMeshProUGUI _text;
        [SerializeField, HideInInspector] CanvasGroup _canvasGroup;

        public TextMeshProUGUI text
        {
            get
            {
                if (_text == null)
                    _text = GetComponent<TextMeshProUGUI>();
                if (_text == null)
                    _text = GetComponentInChildren<TextMeshProUGUI>();
                return _text;
            }
        }
        
        /// <summary>
        /// This is for Text Animation Fade
        /// </summary>
        public virtual float liveTime { get; }

        public virtual CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                    _canvasGroup = GetComponentInParent<CanvasGroup>();
                return _canvasGroup;
            }
        }

        protected virtual void Awake()
        {
            _text = text;
        }

        protected virtual void OnValidate()
        {
            if (_text == null) _text = text;
            if (_canvasGroup == null) _canvasGroup = CanvasGroup;
        }
    }
}
