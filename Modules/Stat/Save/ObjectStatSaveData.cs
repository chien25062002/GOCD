using System;
using System.Collections.Generic;
using UnityEngine;

namespace GOCD.Framework.Module.Stat
{
    [Serializable]
    public class ObjectStatSaveData
    {
        public string ObjectId;
        public List<StatModifierSaveData> Modifiers = new();
    }
}
