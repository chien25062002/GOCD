using DG.Tweening;
using System;

namespace GOCD.Framework
{
    [Serializable]
    public abstract class ViewTransition
    {
        public virtual string displayName { get; }

        public virtual Tween GetTween(ViewTransitionEntity entity, float duration)
        {
            return null;
        }
    }
}
