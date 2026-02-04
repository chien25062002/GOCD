using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CodeSketch.Modules.StatSystem
{
    public static class StatDataIO
    {
        public static void SaveToDisk(string objectId, List<StatModifierSaveData> modifiers)
        {
            var path = Path.Combine(Application.persistentDataPath, $"stat_{objectId}.json");

            var data = new ObjectStatSaveData
            {
                ObjectId = objectId,
                Modifiers = modifiers
            };

            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
        }

        public static List<StatModifierSaveData> LoadFromDisk(string objectId)
        {
            var path = Path.Combine(Application.persistentDataPath, $"stat_{objectId}.json");

            if (!File.Exists(path))
                return null;

            var json = File.ReadAllText(path);
            var data = JsonUtility.FromJson<ObjectStatSaveData>(json);
            return data.Modifiers;
        }
    }
}