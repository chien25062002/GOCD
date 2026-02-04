
using System;
using UnityEngine;

namespace CodeSketch.Modules.StatSystem
{
    public class PercentModifier : IStatModifier, IOverTimeModifier
    {
        public string StatId { get; }
        public Guid SourceId { get; }
        public float PercentValue { get; }
        public float Duration { get; }
        public float Elapsed { get; set; }

        public StatModifierType ModifierType => Duration > 0f ? StatModifierType.PercentOverTime : StatModifierType.Percent;
        public float Value => PercentValue;
        public bool IsExpired => Elapsed >= Duration;

        public PercentModifier(string statId, float percentValue, Guid sourceId, float duration = 0f)
        {
            StatId = statId;
            PercentValue = percentValue / 100f;
            SourceId = sourceId;
            Duration = duration;
        }

        public void Apply(IStatController controller, float deltaTime)
        {
            float baseValue = controller.GetBaseValue(StatId);
            float totalAmount = baseValue * PercentValue;

            if (Duration > 0f)
            {
                float step = (totalAmount / Duration) * deltaTime;
                controller.ModifyStat(StatId, step, suppressEvent: true);
                Elapsed += deltaTime;
            }
            else
            {
                controller.ModifyStat(StatId, totalAmount);
                Elapsed = Duration;
            }
        }
    }
}
