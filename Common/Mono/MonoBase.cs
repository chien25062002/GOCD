using UnityEngine;

namespace GOCD.Framework
{
    public class MonoBase : MonoCached
    {
        protected virtual void OnEnable()
        {
            MonoCallback.Instance.EventUpdate += Tick;
            MonoCallback.Instance.EventLateUpdate += LateTick;
            MonoCallback.Instance.EventFixedUpdate += FixedTick;
        }

        protected virtual void OnDisable()
        {
            if (MonoCallback.IsDestroyed)
                return;

            MonoCallback.Instance.EventUpdate -= Tick;
            MonoCallback.Instance.EventLateUpdate -= LateTick;
            MonoCallback.Instance.EventFixedUpdate -= FixedTick;
        }

        protected virtual void Tick()
        {
        }

        protected virtual void LateTick()
        {
        }

        protected virtual void FixedTick()
        {
        }
    }
}