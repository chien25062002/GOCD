using System;
using GOCD.Framework.Diagnostics;
using UnityEngine;

namespace GOCD.Framework
{
    /// <summary>
    /// Helper dùng để khởi tạo hệ thống collect (thu thập), tìm điểm đến phù hợp và spawn collect container.
    /// </summary>
    public static class GOCDCollectHelper
    {
        /// <summary>
        /// Spawn một collect container với config, số lượng và vị trí spawn chỉ định.
        /// </summary>
        /// <param name="config">Config chứa thông tin bước và prefab collect</param>
        /// <param name="count">Số lượng phần tử cần thu thập</param>
        /// <param name="spawnPosition">Vị trí spawn trong thế giới</param>
        /// <param name="onComplete">Callback khi collect hoàn tất</param>
        public static void Spawn(GOCDCollectConfig config, int count, Vector3 spawnPosition, Action onComplete = null)
        {
            // Tìm điểm đích tương ứng với config
            GOCDCollectDestination destination = GOCDCollectDestinationHelper.Get(config);

            // Nếu không tìm được thì log và trả về
            if (destination == null)
            {
                GOCDDebug.Log(typeof(GOCDCollectHelper), $"Can't find destination for {config.name}");
                onComplete?.Invoke();
                return;
            }

            // Tạo container rỗng để chứa các collect item
            GameObject objCollect = new GameObject(config.name, typeof(RectTransform));

            // Gán component chính
            GOCDCollect collect = objCollect.AddComponent<GOCDCollect>();

            // Thiết lập vị trí cha, vị trí xuất hiện và thứ tự
            collect.TransformCached.SetParent(destination.TransformCached.parent, false);
            collect.TransformCached.SetAsLastSibling();
            collect.TransformCached.position = spawnPosition;

            // Bắt đầu quá trình collect
            collect.Construct(config, destination, count, onComplete);
        }
    }
}