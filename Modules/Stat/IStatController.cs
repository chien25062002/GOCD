using UnityEngine;

namespace GOCD.Framework.Module.Stat
{
    public interface IStatController
    {
        void ModifyStat(string statId, float value);
        void ModifyStat(string statId, float value, bool suppressEvent);
        float GetStat(string statId);
        float GetBaseValue(string statId);  // ✅ Thêm dòng này!
    }
}
