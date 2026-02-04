using UnityEngine;

namespace CodeSketch.Modules.StatSystem
{
    public static class StatModifierFactory
    {
        public static IStatModifier FromSaveData(StatModifierSaveData save)
        {
            return save.ModifierType switch
            {
                StatModifierType.Flat => new FlatModifier(save.StatId, save.Value, save.SourceId),
                StatModifierType.OverTime => new OverTimeModifier(save.StatId, save.Value, save.Duration, save.SourceId)
                {
                    Elapsed = save.Elapsed
                },
                StatModifierType.Percent => new PercentModifier(save.StatId, save.Value, save.SourceId, 0f),
                StatModifierType.PercentOverTime => new PercentModifier(save.StatId, save.Value, save.SourceId, save.Duration)
                {
                    Elapsed = save.Elapsed
                },
                StatModifierType.FlatOverTime => new FlatOverTimeModifier(save.StatId, save.Value, save.Duration, save.SourceId)
                {
                    Elapsed = save.Elapsed
                },
                _ => throw new System.NotImplementedException()
            };
        }
    }
}