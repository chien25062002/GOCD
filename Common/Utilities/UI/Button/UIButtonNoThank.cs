using System;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace GOCD.Framework
{
    public class UIButtonNoThank : UIButtonBase
    {
        [SerializeField] Popup _popup;
        
        [Title("Config")]
        [SerializeField] float _delay = 0.5f;
        [SerializeField] float _scaleDuration = 0.25f;
        [SerializeField] Ease _ease = Ease.Linear;

        public UnityEvent OnClick;
        public event Action ActionOnClick;

        Tween _tweenDelay;
        Tween _tweenScale;
        
        protected override void Awake()
        {
            base.Awake();
            if (_popup == null)
                _popup = GetComponentInParent<Popup>();
                
            TransformCached.localScale = Vector3.zero;
            _tweenDelay = Tween.Delay(_popup.openDuration + _delay, Show);
        }

        void OnDestroy()
        {
            _tweenDelay.Stop();
            _tweenScale.Stop();
        }

        void Show()
        {
            _tweenScale = Tween.Scale(TransformCached, 1f, _scaleDuration, ease: _ease);
        }

        public override void Button_OnClick()
        {
            base.Button_OnClick();
            button.interactable = false;
            
            OnClick?.Invoke();
            ActionOnClick?.Invoke();
            
            _popup.Close();
        }
    }  
}
