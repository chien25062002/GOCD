using DG.Tweening;
using UnityEngine;

namespace GOCD.Framework
{
    [System.Serializable]
    public class TextAnimationFade : TextAnimation
    {
        public override string displayName => "Fade";
        
        [SerializeField] float _fadeStart = 0f;
        [SerializeField] float _fadeEnd = 1f;
        [SerializeField] AnimationCurve _curve = AnimationCurve.Linear(0, 0, 1, 1);
        
        protected override Tween GetTween(UI_TextBase_Floating textBase, float duration)
        {
            return textBase.CanvasGroup.DOFade(_fadeEnd, duration)
                .SetEase(_curve)
                .ChangeStartValue(_fadeStart);
        }

        protected override PrimeTween.Tween GetPrimeTween(UI_TextBase_Floating textBase, float duration)
        {
            var canvasGroup = textBase.CanvasGroup;

            return PrimeTween.Tween.Alpha(
                canvasGroup,
                _fadeStart,
                _fadeEnd,
                duration,
                PrimeTween.Easing.Curve(_curve)
            );
        }
    }
}