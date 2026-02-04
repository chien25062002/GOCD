
using System;
using UnityEngine;

namespace CodeSketch.Modules.StatSystem
{
    public class OverTimeModifier : IStatModifier, IOverTimeModifier
    {
        public string StatId { get; }
        public Guid SourceId { get; }
        public float Value { get; }
        public float Duration { get; }
        public float Elapsed { get; set; }

        public StatModifierType ModifierType => StatModifierType.OverTime;
        public bool IsExpired => Elapsed >= Duration;

        public OverTimeModifier(string statId, float value, float duration, Guid sourceId)
        {
            StatId = statId;
            Value = value;
            Duration = duration;
            SourceId = sourceId;
        }

        public void Apply(IStatController controller, float deltaTime)
        {
            if (IsExpired) return;

            float step = (Value / Duration) * deltaTime;
            controller.ModifyStat(StatId, step, suppressEvent: true);
            Elapsed += deltaTime;
        }
    }
}
