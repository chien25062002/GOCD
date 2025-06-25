using DG.Tweening;
using System;
using UnityEngine;

namespace GOCD.Framework
{
    /// <summary>
    /// Thành phần chính đại diện cho một collect container (tập hợp các item được spawn và thu thập về điểm đích).
    /// </summary>
    public class GOCDCollect : MonoCached
    {
        GOCDCollectConfig _config;
        GOCDCollectDestination _destination;
        Tween _tween;
        Action _onComplete;

        void OnDestroy()
        {
            _tween?.Kill();
        }

        /// <summary>
        /// Khởi tạo collect container với config, điểm đến và callback hoàn thành.
        /// </summary>
        public void Construct(GOCDCollectConfig config, GOCDCollectDestination destination, int count, Action onComplete)
        {
            _config = config;
            _destination = destination;
            _onComplete = onComplete;

            // Tính số lượng item cần spawn dựa vào giá trị input
            int spawnCount = config.GetSpawnCount(count);

            // Tính khoảng cách thời gian giữa các lần spawn
            float delayBetween = spawnCount > 1 ? config.spawnDuration / (spawnCount - 1) : 0.0f;

            // Tạo sequence để lần lượt spawn các item
            Sequence sequence = DOTween.Sequence();

            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 spawnPosition = config.spawnPositions.GetLoop(i);

                sequence.AppendCallback(() => { Spawn(spawnPosition); });
                sequence.AppendInterval(delayBetween);
            }

            sequence.Play();
            sequence.OnComplete(StartCheckEmptyLoop);

            _tween?.Kill();
            _tween = sequence;

            // Thông báo điểm đến rằng quá trình thu thập bắt đầu
            _destination.ReturnBegin(count, spawnCount);
        }

        /// <summary>
        /// Bắt đầu vòng lặp kiểm tra container rỗng (để huỷ tự động sau khi item hoàn tất).
        /// </summary>
        void StartCheckEmptyLoop()
        {
            _tween?.Kill();
            _tween = DOVirtual.DelayedCall(1.0f, null, false)
                              .SetLoops(-1, LoopType.Restart)
                              .OnUpdate(CheckEmpty);
        }

        /// <summary>
        /// Nếu không còn item nào là huỷ collect container.
        /// </summary>
        void CheckEmpty()
        {
            if (TransformCached.childCount == 0)
                Destruct();
        }

        /// <summary>
        /// Huỷ collect container, gọi onComplete và thông báo điểm đến kết thúc.
        /// </summary>
        void Destruct()
        {
            _destination.ReturnEnd();
            _onComplete?.Invoke();
            Destroy(GameObjectCached);
        }

        /// <summary>
        /// Spawn một item tại vị trí chỉ định và truyền thông tin cấu hình.
        /// </summary>
        void Spawn(Vector3 spawnPosition)
        {
            GOCDCollectItem item = _config.spawnPrefab.Create(TransformCached, false).GetComponent<GOCDCollectItem>();
            item.TransformCached.localPosition = spawnPosition;
            item.Construct(_config, _destination);
        }
    }
}
