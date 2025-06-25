using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace GOCD.Framework
{
    public class AnimationSequenceStepGraphicColor : AnimationSequenceStepAction<Graphic>
    {
        [SerializeField]
        Color _value = Color.white;

        [SerializeField]
        [ShowIf("@_changeStartValue")]
        Color _valueStart = Color.white;

        public override string displayName { get { return $"{(_isSelf ? "Graphic (This)" : _owner)}: DOColor"; } }

        protected override Tween GetTween(AnimationSequence animationSequence)
        {
            Graphic owner = _isSelf ? animationSequence.graphic : _owner;

            float duration = _isSpeedBased ? Mathf.Abs(_value.Magnitude() - owner.color.Magnitude()) / _duration : _duration;
            Color start = _changeStartValue ? _valueStart : owner.color;
            Color end = _relative ? owner.color + _value : _value;

            Tween tween = owner.DOColor(end, duration)
                               .ChangeStartValue(start);

            owner.color = end;

            return tween;
        }

        protected override Tween GetResetTween(AnimationSequence animationSequence)
        {
            Graphic owner = _isSelf ? animationSequence.graphic : _owner;

            return owner.DOColor(owner.color, 0.0f);
        }
    }
}