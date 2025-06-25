using UnityEngine;

namespace GOCD.Framework.Module.Stat
{
    [System.Serializable]
    public enum StatModifierType
    {
        Flat,
        Percent,
        OverTime,
        PercentOverTime,
        FlatOverTime
    }
}
