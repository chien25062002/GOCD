using DG.Tweening;
using UnityEngine;

namespace GOCD.Framework
{
    public class PopupAnimationScale : PopupAnimation
    {
        [SerializeField] Transform _target;
        [SerializeField] float _scaleStart = 0.3f;
        [SerializeField] float _scaleEnd = 1.0f;
        [SerializeField] Ease _ease = Ease.OutBack;

        public override string displayName => "Scale";

        protected override Tween GetTween(Popup popup, float duration)
        {
            Transform transform = _target == null ? popup.TransformCached : _target;

            return transform.DOScale(_scaleEnd, duration)
                            .SetEase(_ease)
                            .ChangeStartValue(Vector3.one * _scaleStart);
        }
    }
}
