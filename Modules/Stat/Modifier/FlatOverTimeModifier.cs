
using System;
using UnityEngine;

namespace CodeSketch.Modules.StatSystem
{
    public class FlatOverTimeModifier : IStatModifier, IOverTimeModifier
    {
        public string StatId { get; }
        public Guid SourceId { get; }
        public float Value { get; }
        public float Duration { get; }
        public float Elapsed { get; set; }

        private bool _applied = false;

        public StatModifierType ModifierType => StatModifierType.FlatOverTime;
        public bool IsExpired => Elapsed >= Duration;

        public FlatOverTimeModifier(string statId, float value, float duration, Guid sourceId)
        {
            StatId = statId;
            Value = value;
            Duration = duration;
            SourceId = sourceId;
        }

        public void Apply(IStatController controller, float deltaTime)
        {
            if (!_applied)
            {
                controller.ModifyStat(StatId, Value, suppressEvent: true);
                _applied = true;
            }

            Elapsed += deltaTime;

            if (IsExpired)
            {
                controller.ModifyStat(StatId, -Value, suppressEvent: true);
            }
        }
    }
}
