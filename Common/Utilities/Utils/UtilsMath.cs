using System;
using System.Collections.Generic;
using GOCD.Framework;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game
{
    public class World_Config : ScriptableObject
    {
        [MinValue(1)]
        public int ID = 1;
        public string Name;

        public string MinStuds = "0";
        public string MaxStuds = "0";
        public float MaxLuck = 5;

        #region Cache

        [NonSerialized] double _minStuds = -1;
        [SerializeField] double _maxStuds = -1;

        #endregion

        public List<World_Fish_Config> Fishes = new List<World_Fish_Config>();

        public double MinStudsVal
        {
            get
            {
                if (_minStuds < 0) _minStuds = UtilsNumber.ParseCompactNumber(MinStuds);
                return _minStuds;
            }
        }

        public double MaxStudsVal
        {
            get
            {
                if (_maxStuds < 0) _maxStuds = UtilsNumber.ParseCompactNumber(MaxStuds);
                return _maxStuds;
            }
        }

        public float LuckyEvaluate(double studs)
        {
            return MaxLuck * UtilsMath.To01F(studs, MaxStudsVal);
        }

        [Button]
        public void Validate()
        {
        #if UNITY_EDITOR
            double min = UtilsNumber.ParseCompactNumber(MinStuds);
            double max = UtilsNumber.ParseCompactNumber(MaxStuds);
        
            int count = Fishes.Count;
            if (count == 0 || max <= min)
            {
                Debug.LogWarning($"⚠️ Invalid studs range or empty fish list in World {ID}.");
                return;
            }
        
            for (int i = 0; i < count; i++)
            {
                double t = (count == 1) ? 0.5 : (double)i / (count - 1); // Nếu chỉ có 1 cá → ở giữa
                double studs = Mathf.Lerp((float)min, (float)max, (float)t);
                Fishes[i].Studs = UtilsNumber.FormatVN_CompactRounded(studs);
            }
        
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log($"✅ Updated {count} fish studs for World {ID}: {MinStuds} → {MaxStuds}");
        #endif
        }
    }
}
