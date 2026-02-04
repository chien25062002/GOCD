using System;
using UnityEngine;

namespace CodeSketch.Modules.StatSystem
{
    public class FlatModifier : IStatModifier
    {
        public string StatId { get; }
        public Guid SourceId { get; }
        public float Value { get; }

        public StatModifierType ModifierType => StatModifierType.Flat;
        public bool IsExpired => true;

        public FlatModifier(string statId, float value, Guid sourceId)
        {
            StatId = statId;
            Value = value;
            SourceId = sourceId;
        }

        public void Apply(IStatController controller, float deltaTime)
        {
            controller.ModifyStat(StatId, Value);
        }
    }
}
