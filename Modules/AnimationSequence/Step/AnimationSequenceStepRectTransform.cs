using Sirenix.OdinInspector;
using UnityEngine;

namespace GOCD.Framework
{
    public abstract class AnimationSequenceStepRectTransform : AnimationSequenceStepAction<RectTransform>
    {
        [SerializeField]
        protected Vector3 _value;

        [SerializeField]
        [ShowIf("@_changeStartValue")]
        protected Vector3 _valueStart;

        [SerializeField]
        [VerticalGroup("Value")]
        protected bool _snapping = false;
    }
}