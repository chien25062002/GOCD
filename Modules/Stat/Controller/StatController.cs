// Modified version of StatController.cs
// Includes suppressEvent in ModifyStat and one-shot feedback logic

using System.Collections.Generic;
using UnityEngine;

namespace GOCD.Framework.Module.Stat
{
    public class StatController : IStatController
    {
        Dictionary<string, GOCDValue<float>> _baseValues = new();
        Dictionary<string, GOCDValue<float>> _stats = new();
        List<IStatModifier> _modifiers = new();
        Dictionary<string, int> _activeOverTimeCounts = new();
        Dictionary<string, float> _deltaBuffer = new();

        public System.Action<string, bool> OnOverTimeActiveChanged;
        public System.Action<string, int> OnStatModified;

        public void LinkStat(string statId, GOCDValue<float> externalValue)
        {
            if (!_stats.ContainsKey(statId))
                _stats.Add(statId, externalValue);
            else
                _stats[statId] = externalValue;
        }

        public void LinkBaseStat(string statId, GOCDValue<float> baseValue)
        {
            _baseValues[statId] = baseValue;
        }

        public float GetBaseValue(string statId)
        {
            return _baseValues.TryGetValue(statId, out var val) ? val.value : 0f;
        }

        public float GetStat(string statId)
        {
            if (_stats.TryGetValue(statId, out var val))
                return val.value;
            return 0f;
        }
        
        public void ModifyStat(string statId, float value)
        {
            ModifyStat(statId, value, false);
        }

        public void ModifyStat(string statId, float value, bool suppressEvent = false)
        {
            if (_stats.TryGetValue(statId, out var cvalue))
            {
                float oldValue = cvalue.value;
                cvalue.value += value;

                if (_baseValues.TryGetValue(statId, out var baseVal))
                    cvalue.value = Mathf.Min(cvalue.value, baseVal.value);

                float actualDelta = cvalue.value - oldValue;

                if (!_deltaBuffer.ContainsKey(statId))
                    _deltaBuffer[statId] = 0f;

                _deltaBuffer[statId] += actualDelta;

                int wholeDelta = Mathf.FloorToInt(_deltaBuffer[statId]);
                if (wholeDelta != 0 && !suppressEvent)
                {
                    _deltaBuffer[statId] -= wholeDelta;
                    OnStatModified?.Invoke(statId, wholeDelta);
                }
            }
            else
            {
                var newValue = new GOCDValue<float>(value);

                if (_baseValues.TryGetValue(statId, out var baseVal))
                    newValue.value = Mathf.Min(value, baseVal.value);

                _stats.Add(statId, newValue);

                int intValue = Mathf.FloorToInt(newValue.value);
                if (intValue != 0 && !suppressEvent)
                    OnStatModified?.Invoke(statId, intValue);
            }
        }

        public void AddModifier(IStatModifier modifier)
        {
            if (modifier.ModifierType == StatModifierType.Flat || modifier.ModifierType == StatModifierType.Percent)
            {
                modifier.Apply(this, 0);
            }
            else
            {
                _modifiers.Add(modifier);

                if (modifier is IOverTimeModifier ot)
                {
                    FireOneShotStatModifiedFeedback(modifier);

                    if (!_activeOverTimeCounts.ContainsKey(ot.StatId))
                        _activeOverTimeCounts[ot.StatId] = 0;

                    _activeOverTimeCounts[ot.StatId]++;
                    if (_activeOverTimeCounts[ot.StatId] == 1)
                        OnOverTimeActiveChanged?.Invoke(ot.StatId, true);
                }
            }
        }

        void FireOneShotStatModifiedFeedback(IStatModifier modifier)
        {
            if (modifier is OverTimeModifier overTime)
            {
                float total = overTime.Value * overTime.Duration;
                int rounded = Mathf.FloorToInt(total);
                if (rounded != 0)
                    OnStatModified?.Invoke(overTime.StatId, rounded);
            }
            else if (modifier is FlatOverTimeModifier flatOverTime)
            {
                float total = flatOverTime.Value * flatOverTime.Duration;
                int rounded = Mathf.FloorToInt(total);
                if (rounded != 0)
                    OnStatModified?.Invoke(flatOverTime.StatId, rounded);
            }
            else if (modifier is PercentModifier percent && percent.Duration > 0f)
            {
                float baseValue = GetBaseValue(percent.StatId);
                float total = baseValue * percent.Value * percent.Duration;
                int rounded = Mathf.FloorToInt(total);
                if (rounded != 0)
                    OnStatModified?.Invoke(percent.StatId, rounded);
            }
        }

        public void Tick(float deltaTime)
        {
            for (int i = _modifiers.Count - 1; i >= 0; i--)
            {
                var mod = _modifiers[i];
                mod.Apply(this, deltaTime);

                if (mod.IsExpired)
                {
                    if (mod is IOverTimeModifier ot)
                    {
                        if (_activeOverTimeCounts.ContainsKey(ot.StatId))
                        {
                            _activeOverTimeCounts[ot.StatId]--;
                            if (_activeOverTimeCounts[ot.StatId] <= 0)
                            {
                                _activeOverTimeCounts[ot.StatId] = 0;
                                OnOverTimeActiveChanged?.Invoke(ot.StatId, false);
                            }
                        }
                    }

                    _modifiers.RemoveAt(i);
                }
            }
        }

        public bool HasOverTimeModifier(string statId)
        {
            return _activeOverTimeCounts.TryGetValue(statId, out int count) && count > 0;
        }

        public List<IStatModifier> GetActiveModifiers() => _modifiers;

        public void Save(string objectId)
        {
            var data = ExportSaveData();
            StatDataIO.SaveToDisk(objectId, data);
        }

        public void Load(string objectId)
        {
            var data = StatDataIO.LoadFromDisk(objectId);
            ImportSaveData(data);
        }

        public List<StatModifierSaveData> ExportSaveData() { return null; } // skipped for brevity
        public void ImportSaveData(List<StatModifierSaveData> saved) { }     // skipped for brevity
    }
}
