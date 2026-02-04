using System;

namespace CodeSketch.Modules.StatSystem
{
    [Serializable]
    public class StatModifierSaveData
    {
        public string StatId;
        public Guid SourceId;
        public StatModifierType ModifierType;
        public float Value;
        public float Duration;
        public float Elapsed;
    }
}
