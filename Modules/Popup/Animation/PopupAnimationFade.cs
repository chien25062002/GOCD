using DG.Tweening;
using UnityEngine;

namespace GOCD.Framework
{
    public class PopupAnimationFade : PopupAnimation
    {
        public override string displayName => "Fade";

        protected override Tween GetTween(Popup popup, float duration)
        {
            CanvasGroup canvasGroup = popup.canvasGroup;

            return canvasGroup.DOFade(1f, duration)
                              .ChangeStartValue(0.0f);
        }
    }
}
