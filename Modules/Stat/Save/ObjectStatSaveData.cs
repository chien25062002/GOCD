using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeSketch.Modules.StatSystem
{
    [Serializable]
    public class ObjectStatSaveData
    {
        public string ObjectId;
        public List<StatModifierSaveData> Modifiers = new();
    }
}
