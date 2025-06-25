using System;
using UnityEngine;

namespace GOCD.Framework.LifetimeBinding
{
    public class LifetimeBinding : MonoBehaviour
    {
        public event Action EventRelease;
        
        void OnDestroy()
        {
            EventRelease?.Invoke();
        }
    }
}
