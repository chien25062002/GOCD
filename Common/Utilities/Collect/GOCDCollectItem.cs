using DG.Tweening;
using UnityEngine;

namespace GOCD.Framework
{
    /// <summary>
    /// Đối tượng đại diện cho item được thu thập và thực hiện chuỗi hiệu ứng animation thông qua DOTween.
    /// </summary>
    public class GOCDCollectItem : MonoCached
    {
        Sequence _sequence;
        GOCDCollectDestination _destination;
        RectTransform _rectTransform;

        /// <summary>
        /// Chuỗi hiệu ứng tween đang chạy.
        /// </summary>
        public Sequence sequence => _sequence;

        /// <summary>
        /// Đích đến của item này (nơi sẽ "trả" về sau khi hoàn tất).
        /// </summary>
        public GOCDCollectDestination Destination => _destination;

        /// <summary>
        /// RectTransform được cache để truy cập nhanh.
        /// </summary>
        public RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        /// <summary>
        /// Hủy chuỗi tween khi đối tượng bị huỷ.
        /// </summary>
        void OnDestroy()
        {
            _sequence?.Kill();
        }

        /// <summary>
        /// Khởi tạo item với config animation và đích đến.
        /// </summary>
        public void Construct(GOCDCollectConfig config, GOCDCollectDestination destination)
        {
            if (destination == null)
            {
                Destruct();
                return;
            }

            _destination = destination;

            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            for (int i = 0; i < config.steps.Length; i++)
            {
                config.steps[i].Apply(this);
            }

            _sequence.OnComplete(() =>
            {
                _destination.Return();
                Destruct();
            });
        }

        /// <summary>
        /// Hủy gameObject khi hoàn tất.
        /// </summary>
        void Destruct()
        {
            Destroy(GameObjectCached);
        }
    }
}
