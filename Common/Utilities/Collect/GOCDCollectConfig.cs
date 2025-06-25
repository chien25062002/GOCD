using Sirenix.OdinInspector;
using System.Collections.Generic;
using GOCD.Framework.Diagnostics;
using UnityEngine;

namespace GOCD.Framework
{
    /// <summary>
    /// ScriptableObject chứa cấu hình cho hiệu ứng thu thập item (collect).
    /// Gồm prefab, thời gian spawn, vị trí spawn mẫu, và các bước thực hiện tween.
    /// </summary>
    [System.Serializable]
    public class GOCDCollectConfig : ScriptableObject
    {
        [Title("Spawn")]
        [AssetsOnly]
        [SerializeField] GameObject _spawnPrefab;

        [SerializeField] float _spawnDuration = 0.5f;

        [FoldoutGroup("SpawnSample", GroupName = "Spawn Sample (in pixel unit)", Expanded = false)]
        [SerializeField] List<Vector3> _spawnSamplePositions;

        [FoldoutGroup("SpawnSample")]
        [SerializeField] int _spawnSampleCount = 10;

        [FoldoutGroup("SpawnSample")]
        [SerializeField] float _spawnSampleRadius = 100.0f;

        [Title("Spawn Count")]
        [SerializeField] int[] _spawnCountInput;

        [VerticalGroup("SpawnCount")]
        [SerializeField] int[] _spawnCountOutput;

        [Title("Step")]
        [ListDrawerSettings(ShowIndexLabels = false, OnBeginListElementGUI = "BeginDrawListElement", OnEndListElementGUI = "EndDrawListElement")]
        [SerializeReference] GOCDCollectStep[] _steps;

        // ---------------- Properties ----------------

        public GameObject spawnPrefab => _spawnPrefab;
        public float spawnDuration => _spawnDuration;
        public List<Vector3> spawnPositions => _spawnSamplePositions;
        public float spawnSampleRadius => _spawnSampleRadius;
        public int[] spawnCountInput => _spawnCountInput;
        public int[] spawnCountOutput => _spawnCountOutput;
        public GOCDCollectStep[] steps => _steps;

#if UNITY_EDITOR

        [Button("Pure Random", Icon = SdfIconType.Dice3Fill), HorizontalGroup("SpawnSample/Random")]
        private void RandomSpawnSamplePosition()
        {
            _spawnSamplePositions = new List<Vector3>();
            for (int i = 0; i < _spawnSampleCount; i++)
            {
                _spawnSamplePositions.Add(Random.insideUnitCircle * _spawnSampleRadius);
            }
        }

        [Button("Halton Random", Icon = SdfIconType.Dice5Fill), HorizontalGroup("SpawnSample/Random")]
        private void RandomSpawnSamplePositionHalton()
        {
            _spawnSamplePositions = new List<Vector3>();
            UtilsHaltonSequence.Reset();

            while (_spawnSamplePositions.Count < _spawnSampleCount)
            {
                UtilsHaltonSequence.Increment(true, true, false);

                Vector3 position = new Vector3(-_spawnSampleRadius, -_spawnSampleRadius) +
                                   (UtilsHaltonSequence.currentPosition * _spawnSampleRadius * 2.0f);

                if (Vector3.Distance(Vector3.zero, position) > _spawnSampleRadius)
                    continue;

                _spawnSamplePositions.Add(position);
            }
        }

        [Button, VerticalGroup("SpawnCount")]
        private void ValidateSpawnCount()
        {
            Validate(_spawnCountInput);
            Validate(_spawnCountOutput);

            void Validate(int[] arr)
            {
                int last = 0;
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = Mathf.Max(arr[i], 1);
                    if (last >= arr[i])
                        arr[i] = last + 1;
                    last = arr[i];
                }
            }
        }

        private void BeginDrawListElement(int index)
        {
            Sirenix.Utilities.Editor.SirenixEditorGUI.BeginBox(_steps[index].displayName);
        }

        private void EndDrawListElement(int index)
        {
            Sirenix.Utilities.Editor.SirenixEditorGUI.EndBox();
        }

#endif

        /// <summary>
        /// Tính toán số lượng item spawn tương ứng với số lượng input dựa trên cấu hình.
        /// </summary>
        public int GetSpawnCount(int inputCount)
        {
            if (inputCount <= 0)
                return 0;

            if (_spawnCountInput.IsNullOrEmpty() || _spawnCountOutput.IsNullOrEmpty())
            {
                GOCDDebug.Log<GOCDCollectConfig>("Spawn count config is invalid!");
                return 0;
            }

            int countIndex = _spawnCountOutput.Length - 1;

            for (int i = 0; i < _spawnCountInput.Length; i++)
            {
                if (inputCount <= _spawnCountInput[i])
                {
                    countIndex = i;
                    break;
                }
            }

            int inputMin = _spawnCountInput.GetClamp(countIndex - 1);
            int inputMax = _spawnCountInput.GetClamp(countIndex);

            int outputMin = _spawnCountOutput.GetClamp(countIndex - 1);
            int outputMax = _spawnCountOutput.GetClamp(countIndex);

            return inputMin >= inputMax
                ? outputMax
                : Mathf.RoundToInt(Mathf.Lerp(outputMin, outputMax, Mathf.InverseLerp(inputMin, inputMax, inputCount)));
        }
    }
}
