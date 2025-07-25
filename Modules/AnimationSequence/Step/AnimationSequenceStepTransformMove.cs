using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GOCD.Framework
{
    public class AnimationSequenceStepTransformMove : AnimationSequenceStepTransform
    {
        [VerticalGroup("Value")]
        [SerializeField] bool _snapping = false;

        public override string displayName => $"{(_isSelf ? "Transform (This)" : _owner)}: DOLocalMove";

        protected override Tween GetTween(AnimationSequence animationSequence)
        {
            Transform owner = _isSelf ? animationSequence.TransformCached : _owner;

            float duration = _isSpeedBased ? Vector3.Distance(_value, owner.localPosition) / _duration : _duration;
            Vector3 start = _changeStartValue ? _valueStart : owner.localPosition;
            Vector3 end = _relative ? owner.localPosition + _value : _value;

            Tween tween = owner.DOLocalMove(end, duration, _snapping)
                               .ChangeStartValue(start);

            tween.SetTarget(owner);

            owner.localPosition = end;

            return tween;
        }

        protected override Tween GetResetTween(AnimationSequence animationSequence)
        {
            Transform owner = _isSelf ? animationSequence.TransformCached : _owner;

            return owner.DOLocalMove(owner.localPosition, 0.0f);
        }
    }
}