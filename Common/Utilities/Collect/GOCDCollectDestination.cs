using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace GOCD.Framework
{
    /// <summary>
    /// Điểm đích (Destination) để các vật thể "Collect" bay đến.
    /// Theo dõi số lượng vật thể cần hoàn thành, phát sự kiện theo tiến trình collect.
    /// </summary>
    public class GOCDCollectDestination : MonoCached
    {
        [Title("Reference")]
        [SerializeField] Transform _target;

        [Title("Config")]
        [SerializeField] GOCDCollectConfig _config;

        public event Action<int> eventReturnBegin;   // Khi bắt đầu collect N vật thể
        public event Action<float> eventReturn;      // Gọi mỗi khi một vật thể collect xong (progress)
        public event Action eventReturnEnd;          // Gọi khi tất cả vật thể collect xong

        int _returnExpect = 0; // Tổng số collect cần nhận về
        int _returnCount = 0;  // Số collect đã hoàn tất

        public GOCDCollectConfig config => _config;

        /// <summary>
        /// Trả về vị trí của đích (target nếu có, nếu không là chính object)
        /// </summary>
        public Vector3 position => _target == null ? TransformCached.position : _target.position;

        void OnEnable()
        {
            GOCDCollectDestinationHelper.Push(this);
        }

        void OnDisable()
        {
            GOCDCollectDestinationHelper.Pop(this);
        }

        /// <summary>
        /// Bắt đầu nhận collect từ một nhóm item
        /// </summary>
        /// <param name="valueCount">Tổng giá trị hoặc số lượng logic</param>
        /// <param name="spawnCount">Số lượng thực thể được spawn</param>
        public void ReturnBegin(int valueCount, int spawnCount)
        {
            _returnExpect += spawnCount;
            eventReturnBegin?.Invoke(valueCount);
        }

        /// <summary>
        /// Gọi khi một vật thể collect đến đích
        /// </summary>
        public void Return()
        {
            _returnCount++;

            if (_returnCount == _returnExpect)
            {
                _returnCount = 0;
                _returnExpect = 0;
                eventReturn?.Invoke(1.0f); // Done 100%
            }
            else
            {
                float progress = (float)_returnCount / _returnExpect;
                eventReturn?.Invoke(progress); // Đang tiến trình
            }
        }

        /// <summary>
        /// Gọi thủ công khi muốn kết thúc toàn bộ quá trình collect
        /// </summary>
        public void ReturnEnd()
        {
            eventReturnEnd?.Invoke();
        }
    }
}
