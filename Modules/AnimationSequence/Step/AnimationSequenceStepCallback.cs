using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace GOCD.Framework
{
    public class AnimationSequenceStepCallback : AnimationSequenceStep
    {
        [SerializeField]
        bool _isInserted;

        [SerializeField]
        [Min(0f), ShowIf("@_isInserted")]
        float _insertTime;

        [SerializeField]
        UnityEvent _callback;

        public override string displayName { get { return "Callback"; } }

        public override void AddToSequence(AnimationSequence animationSequence)
        {
            if (_isInserted)
                animationSequence.sequence.InsertCallback(_insertTime, () => { _callback?.Invoke(); });
            else
                animationSequence.sequence.AppendCallback(() => { _callback?.Invoke(); });
        }
    }
}