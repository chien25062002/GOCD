using UnityEngine;

namespace CodeSketch.Modules.StatSystem
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
