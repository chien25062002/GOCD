using System;
using UnityEngine;

namespace CodeSketch.Modules.StatSystem
{
    public interface IStatModifier
    {
        string StatId { get; }
        Guid SourceId { get; }
        StatModifierType ModifierType { get; }
        float Value { get; }
        bool IsExpired { get; }

        void Apply(IStatController controller, float deltaTime);
    }
    
    public static class StatModifierExtensions
    {
        public static void Log(this IStatModifier modifier)
        {
            Debug.Log($"[Modifier] StatId: {modifier.StatId}, SourceId: {modifier.SourceId}, Type: {modifier.ModifierType}, Value: {modifier.Value}, IsExpired: {modifier.IsExpired}");
        }
    }
}
