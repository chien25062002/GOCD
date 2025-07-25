using DG.Tweening;
using UnityEngine;

namespace GOCD.Framework
{
    public class AnimationSequenceStepInterval : AnimationSequenceStep
    {
        [SerializeField] 
        float _duration;

        public override string displayName { get { return "Interval"; } }

        public override void AddToSequence(AnimationSequence animationSequence)
        {
            animationSequence.sequence.AppendInterval(_duration);
        }
    }
}